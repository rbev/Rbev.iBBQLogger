using System.Collections.ObjectModel;
using System.Reactive.Linq;
using RBev.iBBQLogger.Bluetooth;
using RBev.iBBQLogger.Bluetooth.Model;
using RBev.iBBQLogger.Infrastructure;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace RBev.iBBQLogger.Presentation;

public partial class DevicePageViewModel : BaseViewModel
{
    private readonly IBluetoothService _bluetoothService;
    private InkbirdDevice _device;
    private bool _hasDevice;
    private IDisposable? _streamSubscription;

    [Reactive] public partial string DeviceName { get; set; } = "No device selected";
    [Reactive] public partial string Message { get; set; } = string.Empty;
    [Reactive] public partial bool IsStreaming { get; set; }
    public ObservableCollection<string> ProbeLog { get; } = [];

    public DevicePageViewModel(IScreen screen, IBluetoothService bluetoothService) : base(screen)
    {
        _bluetoothService = bluetoothService;
    }

    public void SetDevice(InkbirdDevice device)
    {
        _device = device;
        _hasDevice = true;
        DeviceName = $"{device.Name} ({device.Id})";
        Message = string.Empty;
        ProbeLog.Clear();
        StopStream();
    }

    [ReactiveCommand]
    private void Start()
    {
        if (!_hasDevice)
        {
            Message = "No device selected.";
            return;
        }

        if (_streamSubscription != null)
            return;

        Message = "Connecting and streaming...";
        IsStreaming = true;

        _streamSubscription = _bluetoothService
            .StreamProbeData(_device, autoReconnect: true)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(probes =>
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss");
                    foreach (var probe in probes)
                    {
                        ProbeLog.Insert(0, $"{timestamp} {probe.Name}: {probe.Temperature:F1} °C");
                    }

                    if (ProbeLog.Count > 500)
                    {
                        ProbeLog.RemoveAt(ProbeLog.Count - 1);
                    }

                    Message = $"Streaming ({probes.Length} probe values in latest update).";
                },
                ex =>
                {
                    Message = ex.ToString();
                    StopStream();
                },
                StopStream);
    }

    [ReactiveCommand]
    private void Stop()
    {
        Message = "Stopped";
        StopStream();
    }

    [ReactiveCommand]
    private void Back()
    {
        StopStream();
        HostScreen.Router.NavigateBack.Execute().Subscribe();
    }

    private void StopStream()
    {
        _streamSubscription?.Dispose();
        _streamSubscription = null;
        IsStreaming = false;
    }
}
