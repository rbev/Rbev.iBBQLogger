using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Aggregation;
using RBev.iBBQLogger.Infrastructure.Charting;

namespace RBev.iBBQLogger.Infrastructure;

public static class DynamicDataExtensions
{

    extension<TObject>(IObservable<IChangeSet<TObject>> source)
        where TObject : notnull
    {
        public IObservable<IChangeSet<TOut>> TransformWithDisposal<TOut>(
            Func<TObject, Action<IDisposable>, TOut> factoryMethod)
            where TOut : notnull
        {
            return source
                .Transform(item =>
                {
                    var compositeDispose = new CompositeDisposable();
                    var newItem = factoryMethod(item, compositeDispose.Add);
                    return new DisposableItem<TOut>(newItem, compositeDispose);
                })
                .DisposeMany()
                .Transform(x => x.Item);
        }
    }

    /// <summary>
    /// Wrapper to convert something that subscribes to observables in it's creation
    /// </summary>
    private class DisposableItem<T>(T item, IDisposable disposer) : IDisposable
    {
        public T Item { get; set; } = item;

        public void Dispose()
        {
            disposer.Dispose();
        }
    }
}