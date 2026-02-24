using Autofac;
using RBev.iBBQLogger.Bluetooth;
using RBev.iBBQLogger.Presentation;
using ReactiveUI;

namespace RBev.iBBQLogger;

public class ContainerSetupModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterReactiveUiIntegration(builder);
        
        builder.RegisterAssemblyTypes(ThisAssembly).AsClosedTypesOf(typeof(IViewFor<>));
        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(x => x is { IsClass: true, IsAbstract: false } && x.Name.EndsWith("ViewModel"))
            .AsSelf();


        //override shell with singleton
        builder.RegisterType<ShellViewModel>()
            .AsSelf().As<IScreen>()
            .SingleInstance();
        
        builder.RegisterType<BluetoothService>().As<IBluetoothService>().SingleInstance();
    }

    private static void RegisterReactiveUiIntegration(ContainerBuilder builder)
    {
        builder.RegisterType<RBev.iBBQLogger.Infrastructure.ViewLocator>()
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}