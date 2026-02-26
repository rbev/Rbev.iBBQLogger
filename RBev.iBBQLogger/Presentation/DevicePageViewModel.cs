using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using DynamicData.Binding;
using RBev.iBBQLogger.Bluetooth;
using RBev.iBBQLogger.Bluetooth.Model;
using RBev.iBBQLogger.Infrastructure;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace RBev.iBBQLogger.Presentation;

public partial class DevicePageViewModel : BaseViewModel
{
    private readonly record struct ProbeLogEntry(DateTime TimeStamp, double? Temperature, string Message);

    private readonly IBluetoothService _bluetoothService;
    private readonly SourceList<ProbeLogEntry> _probeLogEntries = new();

    [Reactive] public partial string DeviceName { get; set; } = "No device selected";
    [Reactive] public partial string Message { get; set; } = string.Empty;
    [Reactive] public partial bool IsStreaming { get; set; }
    [Reactive] public partial InkbirdDevice? Device  { get; set; }
    
    public ReadOnlyObservableCollection<string> ProbeLog { get; }

    public DevicePageViewModel(IScreen screen, IBluetoothService bluetoothService) : base(screen)
    {
        _bluetoothService = bluetoothService;

        _probeLogEntries
            .Connect()
            .Sort(SortExpressionComparer<ProbeLogEntry>.Descending(x => x.TimeStamp))
            .Top(500)
            .Transform(x => string.Format($"{x.TimeStamp:hh:MM:ss}: {x.Message} {x.Temperature:0.00}°C"))
            .Bind(out var probeLog)
            .Subscribe();

        ProbeLog = probeLog;
        
        this.WhenActivated(d =>
        {
            d(StreamData());
        });
    }

    private IDisposable StreamData()
    {
        return this.WhenAnyValue(
                x => x.Device,
                x => x.IsStreaming,
                (device, isStreaming) => isStreaming ? device : null)
            .Select(device =>
            {
                if (device == null)
                    return Observable.Empty<TemperatureProbe[]>();

                return _bluetoothService
                    .StreamProbeData(device.Value, autoReconnect: true);

            })
            .Switch()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(probes =>
                {
                    var now = DateTime.Now;
                    _probeLogEntries.AddRange(probes
                        .Select(probe => new ProbeLogEntry(now,
                            probe.Temperature,
                            probe.Name)));
                },
                ex =>
                {
                    Message = ex.ToString();
                    IsStreaming = false;
                }
            );
    }

    public void SetDevice(InkbirdDevice device)
    {
        DeviceName = $"{device.Name} ({device.Id})";
        Message = string.Empty;
        Device = device;
    }

    [ReactiveCommand]
    private void Start()
    {
        if (Device == null)
        {
            Message = "No device selected.";
            return;
        }

        Message = "Connecting and streaming...";
        IsStreaming = true;
    }

    [ReactiveCommand]
    private void Stop()
    {
        Message = "Stopped";
        IsStreaming = false;
    }

    [ReactiveCommand]
    private void Back()
    {
        HostScreen.Router.NavigateBack.Execute().Subscribe();
    }

}
