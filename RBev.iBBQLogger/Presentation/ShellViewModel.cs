using System.Reactive.Linq;
using Autofac;
using ReactiveUI;

namespace RBev.iBBQLogger.Presentation;

public class ShellViewModel : IScreen, IActivatableViewModel
{
    private readonly ILifetimeScope _container;
    public ViewModelActivator Activator { get; } = new();

    public ShellViewModel(ILifetimeScope container)
    {
        _container = container;
        this.WhenActivated(d =>
        {
            if (Router.NavigationStack.Count == 0)
            {
                d(Router.Navigate.Execute(_container.Resolve<RootPageViewModel>()).Subscribe());
            }
        });
    }

    public RoutingState Router { get; } = new();

    public async Task<T> NavigateAsync<T>(Action<T>? initialize = null) where T : IRoutableViewModel
    {
        var viewModel = _container.Resolve<T>();
        initialize?.Invoke(viewModel);
        await Router.Navigate.Execute(viewModel);
        return viewModel;
    }

}