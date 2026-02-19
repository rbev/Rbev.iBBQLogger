using Autofac;
using Avalonia;
using ReactiveUI.Avalonia;
using Splat.Autofac;

namespace RBev.iBBQLogger.Infrastructure.Avalonia;

public static class AppBuilderExtensions
{
    public static AppBuilder UseReactiveUIWithAutofac(this AppBuilder builder, Action<ContainerBuilder> configure)
        => builder.UseReactiveUIWithDIContainer(
            () =>new ContainerBuilder(),
            configure,
            builder =>
            {
                var autofacResolver = builder.UseAutofacDependencyResolver();
                // Register the resolver in Autofac so it can be later resolved
                builder.RegisterInstance(autofacResolver);
                var container = builder.Build();
                autofacResolver.SetLifetimeScope(container);
                return autofacResolver;
            });
}