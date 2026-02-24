using System.Linq;
using RBev.iBBQLogger.Bluetooth;
using RBev.iBBQLogger.Bluetooth.Model;
using RBev.iBBQLogger.Infrastructure;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace RBev.iBBQLogger.Presentation;

public partial class RootPageViewModel : BaseViewModel
{
    private readonly IBluetoothService _bluetoothService;

    [Reactive] public partial InkbirdDevice[] DeviceList { get; set; } = [];
    [Reactive] public partial TemperatureProbe[] ProbeData { get; set; } = [];
    [Reactive] public partial string Message { get; set; }

    public RootPageViewModel(IScreen screen, IBluetoothService bluetoothService) : base(screen)
    {
        _bluetoothService = bluetoothService;
    }

    [ReactiveCommand]
    private async Task SearchDevicesAsync()
    {
        try
        {
            DeviceList = await _bluetoothService.ScanForDevicesAsync();
        }
        catch (Exception e)
        {
            Message = e.ToString();
        }
    }

    [ReactiveCommand]
    private async Task ReadProbeDataAsync(InkbirdDevice device)
    {
        try
        {
            ProbeData = await _bluetoothService.ReadProbeDataAsync(device);
            Console.WriteLine("Probe Data:");
            ProbeData.ToList().ForEach(p => Console.WriteLine($"Device: {p.DeviceId}, Temperature: {p.Temperature}"));
        }
        catch (Exception e)
        {
            Message = e.ToString();
        }
    }
}