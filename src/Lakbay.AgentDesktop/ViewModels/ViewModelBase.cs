using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lakbay.AgentDesktop.ViewModels;

/// <summary>
/// No WPF types referenced here on purpose — <see cref="INotifyPropertyChanged"/>
/// and everything else a ViewModel needs lives in plain BCL namespaces, so
/// every ViewModel derived from this is unit-testable without a UI thread
/// (ADR-0022).
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
