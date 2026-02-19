using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RBev.iBBQLogger.Presentation;
using ReactiveUI;
using Splat;

namespace RBev.iBBQLogger;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public IContainer Container { get; private set; } = null!;

    public override void OnFrameworkInitializationCompleted()
    {
        Container = CreateContainer();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Shell()
            {
                ViewModel = Container.Resolve<ShellViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IContainer CreateContainer()
    {
        var cb = new ContainerBuilder();
        cb.RegisterModule<ContainerSetupModule>();
        var container = cb.Build();
        
        //put things into reactiveui
        AppLocator.CurrentMutable.Register(() => container.Resolve<IViewLocator>(), typeof(IViewLocator));

        return container;
    }
}