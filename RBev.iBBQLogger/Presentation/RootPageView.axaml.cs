using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;

namespace RBev.iBBQLogger.Presentation;

public partial class RootPageView : ReactiveUserControl<RootPageViewModel>
{
    public RootPageView()
    {
        InitializeComponent();
    }
}