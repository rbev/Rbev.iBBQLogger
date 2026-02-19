using Autofac;
using Avalonia.Controls;
using ReactiveUI;

namespace RBev.iBBQLogger.Infrastructure;

public class ViewLocator(ILifetimeScope scope) : IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null)
    {
        //annoyingly some code just sends you the object and does not give the
        //generic.... so detect that and use typeof
        var viewModelType = typeof(T);
        if (viewModelType == typeof(object) && viewModel != null)
        {
            viewModelType = viewModel.GetType();
        }

        var viewType = typeof(IViewFor<>).MakeGenericType(viewModelType);

        var view = contract != null
            ? scope.ResolveNamed(contract, viewType)
            : scope.Resolve(viewType);

        if (view is IViewFor viewFor)
        {
            viewFor.ViewModel = viewModel;
            return viewFor;
        }

        throw new Exception("View not found " + typeof(T));
    }
}