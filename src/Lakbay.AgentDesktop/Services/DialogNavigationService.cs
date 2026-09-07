using System.Windows;

namespace Lakbay.AgentDesktop.Services;

/// <summary>
/// The one place in this app that is allowed to know about WPF's
/// <see cref="Window"/> type. Everything else — ViewModels included — only
/// ever sees <see cref="INavigationService"/> (ADR-0022). The ViewModel ->
/// Window mapping is supplied as a factory delegate from the composition
/// root (App.xaml.cs) rather than hardcoded here, so this class itself
/// stays free of any specific View type.
/// </summary>
public class DialogNavigationService : INavigationService
{
    private readonly Func<Type, object, Window> _windowFactory;

    public DialogNavigationService(Func<Type, object, Window> windowFactory)
    {
        _windowFactory = windowFactory ?? throw new ArgumentNullException(nameof(windowFactory));
    }

    public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : notnull
    {
        var window = _windowFactory(typeof(TViewModel), viewModel);
        window.DataContext = viewModel;
        window.Owner = Application.Current?.MainWindow;
        return window.ShowDialog();
    }
}
