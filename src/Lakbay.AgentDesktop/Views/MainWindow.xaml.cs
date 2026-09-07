using System.Windows;

namespace Lakbay.AgentDesktop;

/// <summary>
/// Code-behind is deliberately empty beyond InitializeComponent - all behavior lives in
/// MainViewModel, resolved and assigned as DataContext from the Unity composition root
/// in App.xaml.cs (ADR-0022).
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
