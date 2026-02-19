namespace RBev.iBBQLogger.Macos;

public static class Program
{
    public static void Main(string[] args)
    {
        NSApplication.Init();
        NSApplication.SharedApplication.Delegate = new AppDelegate(args);
        NSApplication.Main(args);
    }
}