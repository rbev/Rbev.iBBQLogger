using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using RBev.iBBQLogger.Bluetooth;
using RBev.iBBQLogger.Bluetooth.Model;
using RBev.iBBQLogger.Infrastructure;
using RBev.iBBQLogger.Infrastructure.Charting;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace RBev.iBBQLogger.Presentation;

public partial class DevicePageViewModel : BaseViewModel
{
    private readonly record struct ProbeLogEntry(DateTime TimeStamp, double Temperature, string ProbeName);

    private readonly IBluetoothService _bluetoothService;
    private readonly SourceList<ProbeLogEntry> _probeLogEntries = new();
    private DateTime Epoch = DateTime.Now;

    [Reactive] public partial string DeviceName { get; set; } = "No device selected";
    [Reactive] public partial string Message { get; set; } = string.Empty;
    [Reactive] public partial bool IsStreaming { get; set; }
    [Reactive] public partial InkbirdDevice? Device { get; set; }
    [Reactive] public partial Axis[] XAxes { get; set; } = [new TimeSpanAxis(
        TimeSpan.FromMinutes(1), 
        x=> x.ToString("g") )
    {
        Name = "Minutes"
    }];

    [Reactive]
    public partial Axis[] YAxes { get; set; } = [new Axis { Name = "Temperature (C)" }];

    public ReadOnlyObservableCollection<string> ProbeLog { get; }
    [Reactive] public partial IReadOnlyCollection<ReactiveChartSeries> ChartSeries { get; set; }

    public DevicePageViewModel(IScreen screen, IBluetoothService bluetoothService) : base(screen)
    {
        _bluetoothService = bluetoothService;

        var log = _probeLogEntries
            .Connect()
            .Sort(SortExpressionComparer<ProbeLogEntry>.Descending(x => x.TimeStamp))
            .Top(500)
            .Transform(x => $"{x.TimeStamp:hh:mm:ss}: {x.ProbeName} {x.Temperature:0.00}°C")
            .Bind(out var probeLog);
        ProbeLog = probeLog;

        var chart = _probeLogEntries
            .Connect()
            .GroupOn(x => x.ProbeName)
            .TransformWithDisposal((group, d) =>
            {
                d(group
                    .List
                    .Connect()
                    .Transform(p => new TimeSpanPoint(p.TimeStamp - Epoch, p.Temperature))
                    .Bind(out var points)
                    .Subscribe());
                return new ReactiveChartSeries
                {
                    Name = group.GroupKey,
                    Values = points
                };
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToCollection();
            
        //ChartSeriesX = chartSeries;

        this.WhenActivated(d =>
        {
            d(log.Subscribe());
            //d(chart.Subscribe());
            d(chart.BindTo(this, x => x.ChartSeries));
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
        _probeLogEntries.Clear();
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