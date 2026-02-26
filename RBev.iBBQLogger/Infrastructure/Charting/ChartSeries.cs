using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace RBev.iBBQLogger.Infrastructure.Charting;

public partial class ReactiveChartSeries : ReactiveObject
{
    [Reactive]
    public partial string? Name { get; set; }
    
    [Reactive]
    public partial ReadOnlyObservableCollection<ObservablePoint>? Values { get; set; }
}