

using RBev.iBBQLogger.Bluetooth.Model;

namespace RBev.iBBQLogger.Bluetooth;

public interface IBluetoothService : IDisposable
{
    public Task<InkbirdDevice[]> ScanForDevicesAsync(TimeSpan? minSearch = null, TimeSpan? maxSearch = null);
    public Task<TemperatureProbe[]> ReadProbeDataAsync(InkbirdDevice device);
}
