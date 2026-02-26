using System.Reactive;
using System.Reactive.Linq;
using CoreBluetooth;

namespace RBev.iBBQLogger.Bluetooth.Macos;

public static class BluetoothEventObservables
{
    extension(CBCentralManager manager)
    {
        public IObservable<EventPattern<object>> UpdatedStateObservable() =>
            Observable.FromEventPattern(
                h => manager.UpdatedState += h,
                h => manager.UpdatedState -= h
            );

        public IObservable<EventPattern<CBDiscoveredPeripheralEventArgs>> DiscoveredPeripheralObservable() =>
            Observable.FromEventPattern<CBDiscoveredPeripheralEventArgs>(
                h => manager.DiscoveredPeripheral += h,
                h => manager.DiscoveredPeripheral -= h
            );

        public IObservable<EventPattern<CBPeripheralEventArgs>> ConnectedPeripheralObservable() =>
            Observable.FromEventPattern<CBPeripheralEventArgs>(
                h => manager.ConnectedPeripheral += h,
                h => manager.ConnectedPeripheral -= h
            );

        public IObservable<EventPattern<CBPeripheralDiconnectionEventEventArgs>> DidDisconnectPeripheralObservable() =>
            Observable.FromEventPattern<CBPeripheralDiconnectionEventEventArgs>(
                h => manager.DidDisconnectPeripheral += h,
                h => manager.DidDisconnectPeripheral -= h
            );

        public IObservable<EventPattern<CBPeripheralErrorEventArgs>> FailedToConnectPeripheralObservable() =>
            Observable.FromEventPattern<CBPeripheralErrorEventArgs>(
                h => manager.FailedToConnectPeripheral += h,
                h => manager.FailedToConnectPeripheral -= h
            );

        public IObservable<EventPattern<CBPeripheralErrorEventArgs>> DisconnectedPeripheralObservable() =>
            Observable.FromEventPattern<CBPeripheralErrorEventArgs>(
                h => manager.DisconnectedPeripheral += h,
                h => manager.DisconnectedPeripheral -= h
            );
    }

    extension(CBPeripheral peripheral)
    {
        public IObservable<EventPattern<NSErrorEventArgs>> DiscoveredServiceObservable() =>
            Observable.FromEventPattern<NSErrorEventArgs>(
                h => peripheral.DiscoveredService += h,
                h => peripheral.DiscoveredService -= h
            );
        
        public IObservable<EventPattern<CBServiceEventArgs>> DiscoveredCharacteristicsObservable() =>
            Observable.FromEventPattern<CBServiceEventArgs>(
                h => peripheral.DiscoveredCharacteristics += h,
                h => peripheral.DiscoveredCharacteristics -= h
            );
        
        public IObservable<EventPattern<CBCharacteristicEventArgs>> UpdatedCharacterteristicValueObservable() =>
            Observable.FromEventPattern<CBCharacteristicEventArgs>(
                h => peripheral.UpdatedCharacterteristicValue += h,
                h => peripheral.UpdatedCharacterteristicValue -= h
            );
        
        public IObservable<EventPattern<CBDescriptorEventArgs>> UpdatedValueObservable() =>
            Observable.FromEventPattern<CBDescriptorEventArgs>(
                h => peripheral.UpdatedValue += h,
                h => peripheral.UpdatedValue -= h
            );
    }
}
