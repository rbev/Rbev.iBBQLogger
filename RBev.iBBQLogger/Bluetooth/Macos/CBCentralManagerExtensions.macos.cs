using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CoreBluetooth;

namespace RBev.iBBQLogger.Bluetooth.Macos;

public static class CBCentralManagerExtensions
{
    extension(CBCentralManager manager)
    {
        public IObservable<EventPattern<CBDiscoveredPeripheralEventArgs>> ScanForPeripheralsAsync(
            CBUUID[]? peripheralUuids = null)
        {
            return Observable.Create<EventPattern<CBDiscoveredPeripheralEventArgs>>(observer =>
            {
                var eventObserver = manager.DiscoveredPeripheralObservable().Subscribe(observer);

                manager.ScanForPeripherals(peripheralUuids);

                return new CompositeDisposable(
                    eventObserver,
                    Disposable.Create(manager.StopScan)
                );
            });
        }

        public async Task<bool> ConnectPeripheralAsync(CBPeripheral peripheral, TimeSpan? timeout = null)
        {
            if (peripheral.State == CBPeripheralState.Connected) return true;
            return await Observable.Create<bool>(observer =>
                {
                    var connectedObserver = manager
                        .ConnectedPeripheralObservable()
                        .Where(x => x.EventArgs.Peripheral.Identifier.Equals(peripheral.Identifier))
                        .Subscribe(_ => observer.OnNext(true));

                    var failedToConnectObserver = manager
                        .FailedToConnectPeripheralObservable()
                        .Where(x => x.EventArgs.Peripheral.Identifier.Equals(peripheral.Identifier))
                        .Subscribe(_ => observer.OnNext(false));

                    manager.ConnectPeripheral(peripheral);

                    return new CompositeDisposable(connectedObserver, failedToConnectObserver);
                })
                .TakeUntil(Observable.Timer(timeout ?? TimeSpan.FromSeconds(15)))
                .FirstOrDefaultAsync();
        }
    }
}

public static class CBPeripheralExtensions
{
    extension(CBPeripheral manager)
    {
        public IObservable<CBCharacteristic[]?> DiscoverCharacteristicsAsync(CBService service)
        {
            return Observable.Create<CBCharacteristic[]?>(observer =>
            {
                var eventObserver = manager
                    .DiscoveredCharacteristicsObservable()
                    .Where(x => x.EventArgs.Service.UUID == service.UUID)
                    .Select(x => x.EventArgs.Service.Characteristics)
                    .Subscribe(observer);

                manager.DiscoverCharacteristics(service);

                return eventObserver;
            }).FirstOrDefaultAsync();
        }

        public IObservable<CBService[]> DiscoverServicesAsync(CBUUID[]? services = null)
        {
            return Observable.Create<CBService[]>(observer =>
            {
                var eventObserver = manager
                    .DiscoveredServiceObservable()
                    .FirstOrDefaultAsync()
                    .Select(_ => (services == null
                                     ? manager.Services
                                     : manager.Services?.Where(s => services.Contains(s.UUID)).ToArray())
                                 ?? [])
                    .Subscribe(observer);

                manager.DiscoverServices(services);

                return eventObserver;
            });
        }
    }
}