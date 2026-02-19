using CoreBluetooth;
using RBev.iBBQLogger.Bluetooth.Model;

namespace RBev.iBBQLogger.Bluetooth;

public class BluetoothService : IBluetoothService
{
    #if MACOS 
    private Lazy<CBCentralManager> manager = new();
    #endif
    public async Task<InkbirdDevice[]> ScanForDevicesAsync(TimeSpan? minSearch = null, TimeSpan? maxSearch = null)
    {
        var found = new TaskCompletionSource();
        var foundDevices = new List<InkbirdDevice>();

        void OnValueOnDiscoveredPeripheral(object? sender, CBDiscoveredPeripheralEventArgs args)
        {
            if (args.Peripheral.Name == "iBBQ")
            {
                //get the name and id of the device and add it to the list of found devices
                foundDevices.Add(new  InkbirdDevice()
                {
                    Id = args.Peripheral.Identifier.ToString(),
                    Name = args.Peripheral.Name
                    
                });
            }
                
            {
                
            }
            found.TrySetResult();
        }

        manager.Value.DiscoveredPeripheral += OnValueOnDiscoveredPeripheral;

        try
        {

            manager.Value.ScanForPeripherals([]);
            await Task.WhenAny(Task.WhenAll(Task.Delay(minSearch ?? TimeSpan.FromSeconds(1)), found.Task), Task.Delay(maxSearch ?? TimeSpan.FromSeconds(10)));

        }
        catch (Exception)
        {
            manager.Value.DiscoveredPeripheral += OnValueOnDiscoveredPeripheral;

            throw;
        }
        
        return foundDevices.ToArray();
    }


    public void Dispose()
    {
        if (manager.IsValueCreated)
            manager.Value.Dispose();
    }
}