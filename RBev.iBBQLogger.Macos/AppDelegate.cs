using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using RBev.iBBQLogger.Infrastructure.Avalonia;
using ReactiveUI.Avalonia;

namespace RBev.iBBQLogger.Macos;

public sealed class AppDelegate(string[] args) : NSApplicationDelegate
{
    public override void DidFinishLaunching(NSNotification notification)
    {
        // Ensure we're a regular foreground app
        NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;
        var lifetime = new ClassicDesktopStyleApplicationLifetime
        {
            Args = args
        };
        
        BuildAvaloniaApp().SetupWithLifetime(lifetime);
        
        lifetime.Start(args ?? []);
        
        // If MainWindow is set in App.OnFrameworkInitializationCompleted,
        // force it visible + bring app front.
        var w = lifetime.MainWindow;
        if (w is not null)
        {
            w.Show();
            w.Activate();
        }

        NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
    }
    

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI()
            .LogToTrace();

}