namespace Lakbay.AgentDesktop.Services;

/// <summary>
/// Lets a ViewModel trigger navigation/dialogs without referencing WPF's
/// <c>Window</c>/<c>Frame</c> types directly (ADR-0022) — keeps ViewModels
/// unit-testable with a fake implementation and no UI thread/dispatcher.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Shows a modal dialog for the given ViewModel and returns the dialog
    /// result (true = confirmed/OK, false = cancelled, null = closed
    /// without a definitive answer).
    /// </summary>
    bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : notnull;
}
