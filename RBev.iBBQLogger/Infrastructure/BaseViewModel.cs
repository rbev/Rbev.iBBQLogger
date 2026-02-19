using ReactiveUI;

namespace RBev.iBBQLogger.Infrastructure;

public class BaseViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public string? UrlPathSegment { get; private set; } 
    public IScreen HostScreen { get; }
    
    public ViewModelActivator Activator { get; } = new();

    public BaseViewModel(IScreen screen)
    {
        UrlPathSegment = this.GetType().Name;
        HostScreen = screen;
    }

}