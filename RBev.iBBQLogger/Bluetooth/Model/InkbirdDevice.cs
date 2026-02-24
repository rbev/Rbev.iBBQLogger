#if MACOS
using NativeBTDevice = CoreBluetooth.CBPeripheral;
#else
using NativeBTDevice = System.Object;
#endif

namespace RBev.iBBQLogger.Bluetooth.Model;

public readonly record struct InkbirdDevice(
    string Id, 
    string Name,
    NativeBTDevice Device);

public readonly record struct TemperatureProbe(
    string DeviceId, 
    string Name, 
    double Temperature);