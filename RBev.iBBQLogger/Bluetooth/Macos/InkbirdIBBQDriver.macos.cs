using System.Reactive.Linq;
using System.Reactive.Disposables;
using CoreBluetooth;
using RBev.iBBQLogger.Bluetooth.InkBird;
using RBev.iBBQLogger.Bluetooth.Model;
using Stateless;

namespace RBev.iBBQLogger.Bluetooth.Macos;

public class InkbirdIBBQDriver : IDisposable
{
    private readonly CBCentralManager _manager;
    private readonly CBPeripheral _btDevice;
    private readonly StateMachine<State, Trigger> _stateMachine = new(State.New);
    private readonly CompositeDisposable _connectionEventSubscriptions = new();
    private CBService? _iBbqService;
    private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(3);
    private bool _isDisposed;

    public bool AutoReconnectEnabled { get; set; }


    public enum State
    {
        New,
        Disconnected,
        Connecting,
        Connected
    }

    public enum Trigger
    {
        Connect,
        Connected,
        Disconnect
    }

    public InkbirdIBBQDriver(CBCentralManager manager, CBPeripheral btDevice)
    {
        _manager = manager;
        _btDevice = btDevice;
        _stateMachine.Configure(State.New)
            .Permit(Trigger.Connect, State.Connecting);

        _stateMachine.Configure(State.Connecting)
            .Permit(Trigger.Connected, State.Connected)
            .Permit(Trigger.Disconnect, State.Disconnected)
            .OnEntryAsync(OnConnectingAsync, "Connecting to device");

        _stateMachine.Configure(State.Connected)
            .Permit(Trigger.Disconnect, State.Disconnected)
            .OnEntryAsync(OnConnectedAsync, "Connected to device");

        _stateMachine.Configure(State.Disconnected)
            .Permit(Trigger.Connect, State.Connecting)
            .OnEntryAsync(OnDisconnectedAsync, "Disconnecting device");

        _connectionEventSubscriptions.Add(
            _manager.DidDisconnectPeripheralObservable()
                .Where(x => x.EventArgs.Peripheral.Identifier.Equals(_btDevice.Identifier))
                .Subscribe(_event => { _ = HandlePeripheralDisconnectedAsync(); }));

        _connectionEventSubscriptions.Add(
            _manager.DisconnectedPeripheralObservable()
                .Where(x => x.EventArgs.Peripheral.Identifier.Equals(_btDevice.Identifier))
                .Subscribe(_event => { _ = HandlePeripheralDisconnectedAsync(); }));

        _connectionEventSubscriptions.Add(
            _manager.FailedToConnectPeripheralObservable()
                .Where(x => x.EventArgs.Peripheral.Identifier.Equals(_btDevice.Identifier))
                .Subscribe(_event => { _ = HandlePeripheralDisconnectedAsync(); }));
    }

    private async Task HandlePeripheralDisconnectedAsync()
    {
        if (_isDisposed)
            return;

        if (_stateMachine.CanFire(Trigger.Disconnect))
            await _stateMachine.FireAsync(Trigger.Disconnect);
    }

    private async Task OnConnectingAsync()
    {
        try
        {
            // Connect to the peripheral
            if (!await _manager.ConnectPeripheralAsync(_btDevice))
            {
                Console.WriteLine("No device connected");
                await _stateMachine.FireAsync(Trigger.Disconnect);
                return;
            }

            // Find the iBBQ service (FFF0 is the standard iBBQ service UUID)
            var services =
                await _btDevice.DiscoverServicesAsync([CBUUID.FromPartial(IBBQBluetoothDefinition.Services.IBBQ.ID)]);
            _iBbqService = services.FirstOrDefault();
            if (_iBbqService == null)
                throw new Exception("iBBQ service not found");

            await _stateMachine.FireAsync(Trigger.Connected);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            if (_stateMachine.CanFire(Trigger.Disconnect))
                await _stateMachine.FireAsync(Trigger.Disconnect);
        }
    }

    private async Task OnConnectedAsync()
    {
        try
        {
            // Discover characteristics
            var characteristics = await _btDevice.DiscoverCharacteristicsAsync(_iBbqService!) ?? [];
            var accountCharacteristic = characteristics
                .FirstOrDefault(c =>
                    c.UUID.Equals(CBUUID.FromPartial(IBBQBluetoothDefinition.Services.IBBQ.Characteristics.Account)));
            var settingsCharacteristic = characteristics
                .FirstOrDefault(c =>
                    c.UUID.Equals(CBUUID.FromPartial(IBBQBluetoothDefinition.Services.IBBQ.Characteristics.Settings)));
            var realtimeTempCharacteristic = characteristics
                .FirstOrDefault(c =>
                    c.UUID.Equals(
                        CBUUID.FromPartial(IBBQBluetoothDefinition.Services.IBBQ.Characteristics.RealtimeData)));
            if (accountCharacteristic == null || settingsCharacteristic == null || realtimeTempCharacteristic == null)
            {
                throw new Exception("Service is missing characteristics");
            }

            //unlock device
            Console.WriteLine("Unlocking device");
            _btDevice.WriteValue(NSData.FromArray(IBBQBluetoothDefinition.Constants.Credentials),
                accountCharacteristic,
                CBCharacteristicWriteType.WithResponse);

            //setup for logging
            Console.WriteLine("Configuring device");
            _btDevice.WriteValue(NSData.FromArray(IBBQBluetoothDefinition.Constants.Configuration.EnableRealTimeData),
                settingsCharacteristic,
                CBCharacteristicWriteType.WithResponse);
            _btDevice.WriteValue(NSData.FromArray(IBBQBluetoothDefinition.Constants.Configuration.UnitCelsius),
                settingsCharacteristic,
                CBCharacteristicWriteType.WithResponse);

            _btDevice.SetNotifyValue(true, realtimeTempCharacteristic);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            if (_stateMachine.CanFire(Trigger.Disconnect))
                await _stateMachine.FireAsync(Trigger.Disconnect);
        }
    }

    private async Task OnDisconnectedAsync()
    {
        OnDisconnecting();

        if (_isDisposed || !AutoReconnectEnabled)
            return;

        await Task.Delay(_reconnectDelay);

        if (_isDisposed)
            return;

        if (_stateMachine.CanFire(Trigger.Connect))
            await _stateMachine.FireAsync(Trigger.Connect);
    }

    private void OnDisconnecting()
    {
        try
        {
            _manager.CancelPeripheralConnection(_btDevice);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task ConnectAsync()
    {
        if (_btDevice.State == CBPeripheralState.Connected && _stateMachine.State == State.Connected)
            return;

        if (_stateMachine.CanFire(Trigger.Connect))
            await _stateMachine.FireAsync(Trigger.Connect);
    }

    public IObservable<TemperatureProbe[]> TemperatureDataObservable() => _btDevice
        .UpdatedCharacterteristicValueObservable()
        .Where(e => e.EventArgs.Characteristic.UUID.Equals(
            CBUUID.FromPartial(IBBQBluetoothDefinition.Services.IBBQ.Characteristics.RealtimeData)))
        .Select(x =>
        {
            var data = x.EventArgs.Characteristic.Value?.ToArray() ?? [];
            List<TemperatureProbe> probes = new();

            // the data comes in as little endian 16 bit integers
            for (int i = 0; i + 1 < data.Length; i += 2)
            {
                int value = data[i + 1] << 8;
                value |= data[i];
                if (value > IBBQBluetoothDefinition.Constants.ProbeErrorValue)
                    continue;

                probes.Add(new TemperatureProbe(_btDevice.Identifier.ToString(), $"Probe {i / 2 + 1}", value / 10d));
            }

            return probes.ToArray();
        })
        .Where(x => x.Length > 0);

    public static InkbirdIBBQDriver Create(CBCentralManager manager, CBPeripheral btDevice) => new(manager, btDevice);

    public void Dispose()
    {
        _isDisposed = true;
        AutoReconnectEnabled = false;
        _connectionEventSubscriptions.Dispose();

        if (_stateMachine.CanFire(Trigger.Disconnect))
            _stateMachine.Fire(Trigger.Disconnect);
    }
}
