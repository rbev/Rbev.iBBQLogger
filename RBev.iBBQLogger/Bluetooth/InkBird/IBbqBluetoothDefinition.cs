namespace RBev.iBBQLogger.Bluetooth.InkBird;

// ReSharper disable once InconsistentNaming
public class IBBQBluetoothDefinition
{
    public static class Constants
    {
        ///<summary>
        /// credentials - used to "enable" the device, otherwise it boots you out when you enable notifications
        /// </summary>
        public static byte[] Credentials =
            [0x21, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0xb8, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00];
        
        
        public const uint ProbeErrorValue = 6550;

        public static class Configuration
        {
            /// <summary>
            /// config data - to enable real time data
            /// </summary>
            public static readonly byte[] EnableRealTimeData = [0x0B, 0x01, 0x00, 0x00, 0x00, 0x00];

            /// <summary>
            /// config data - to enable real time data
            /// </summary>
            public static readonly byte[] UnitCelsius = [0x02, 0x00, 0x00, 0x00, 0x00, 0x00];
        }
    }

    public static class Services
    {
        public static class IBBQ
        {
            public const int ID = 0xfff0;

            public static class Characteristics
            {
                public const int Account = 0xfff2;
                public const int Settings = 0xfff5;
                public const int RealtimeData = 0xfff4;
            }
        }
    }
}
