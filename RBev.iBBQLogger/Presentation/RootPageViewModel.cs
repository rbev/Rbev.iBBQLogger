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
    [Reactive] public partial string Message { get; set; } = string.Empty;

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
    private async Task OpenDeviceScreenAsync(InkbirdDevice device)
    {
        try
        {
            if (HostScreen is not ShellViewModel shellViewModel)
            {
                Message = "Navigation host is not available.";
                return;
            }

            await shellViewModel.NavigateAsync<DevicePageViewModel>(vm => vm.SetDevice(device));
        }
        catch (Exception e)
        {
            Message = e.ToString();
        }
    }
}
