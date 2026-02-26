using RBev.iBBQLogger.Bluetooth.Model;

namespace RBev.iBBQLogger.Bluetooth;

public class BluetoothService : IBluetoothService
{
    public Task<InkbirdDevice[]> ScanForDevicesAsync(TimeSpan? minSearch = null, TimeSpan? maxSearch = null)
    {
        throw new NotImplementedException();
    }

    public Task<TemperatureProbe[]> ReadProbeDataAsync(InkbirdDevice device)
    {
        throw new NotImplementedException();
    }

    public IObservable<TemperatureProbe[]> StreamProbeData(InkbirdDevice device, bool autoReconnect = true)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }
}
