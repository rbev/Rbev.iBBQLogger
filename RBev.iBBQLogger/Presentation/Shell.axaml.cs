using ReactiveUI;
using ReactiveUI.Avalonia;

namespace RBev.iBBQLogger.Presentation;

public partial class Shell : ReactiveWindow<ShellViewModel>
{
    public Shell()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            d(this.Bind(ViewModel, vm => vm.Router, v => v.Host.Router));

            this.Info.Text = $"""
                             ProcessPath: {Environment.ProcessPath}
                             BaseDirectory: {AppContext.BaseDirectory}
                             """;
        });
    }
}