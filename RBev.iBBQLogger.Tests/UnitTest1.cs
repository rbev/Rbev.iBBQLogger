using RBev.iBBQLogger.Bluetooth;

namespace RBev.iBBQLogger.Tests;

public class BleSpike
{
        [Test]
        public async Task FindDevice()
        {
            using var svc = new BluetoothService();
            var devices = await svc.ScanForDevicesAsync();
            
            await Verify(devices);
        }
}