using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CoreBluetooth;
using RBev.iBBQLogger.Bluetooth.Macos;
using RBev.iBBQLogger.Bluetooth.Model;

namespace RBev.iBBQLogger.Bluetooth;

public class BluetoothService : IBluetoothService
{
    private readonly Lazy<CBCentralManager> _manager = new();
    
    public async Task<InkbirdDevice[]> ScanForDevicesAsync(TimeSpan? minSearch = null, TimeSpan? maxSearch = null) =>
        await _manager.Value
            .ScanForPeripheralsAsync()
            .Where(x => x.EventArgs.Peripheral.Name == "iBBQ")
            .Select(x => new InkbirdDevice(x.EventArgs.Peripheral.Identifier.ToString(),
                x.EventArgs.Peripheral.Name ?? "",
                x.EventArgs.Peripheral)
            )
            //todo: make it wait for the minimum search time & return multiple
            .Take(1)
            .Timeout(maxSearch ?? TimeSpan.FromSeconds(10))
            .ToArray();

    public async Task<TemperatureProbe[]> ReadProbeDataAsync(InkbirdDevice device)
    {
        return await StreamProbeData(device, autoReconnect: false)
            .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(30)))
            .FirstOrDefaultAsync() ?? [];
    }

    public IObservable<TemperatureProbe[]> StreamProbeData(InkbirdDevice device, bool autoReconnect = true) =>
        Observable.Using(
            () => InkbirdIBBQDriver.Create(_manager.Value, device.Device),
            driver => driver.TemperatureDataObservable(autoReconnect)
        );

    public void Dispose()
    {
        if (_manager.IsValueCreated)
            _manager.Value.Dispose();
    }
}
