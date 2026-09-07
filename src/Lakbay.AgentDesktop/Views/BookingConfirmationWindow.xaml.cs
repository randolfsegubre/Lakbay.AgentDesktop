using System.Windows;
using Lakbay.AgentDesktop.ViewModels;

namespace Lakbay.AgentDesktop;

/// <summary>
/// The one place allowed to hold a Window reference for this dialog (ADR-0022) - wires
/// BookingConfirmationViewModel.RequestClose to actually closing the window, since the
/// ViewModel itself never references Window directly.
/// </summary>
public partial class BookingConfirmationWindow : Window
{
    public BookingConfirmationWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is BookingConfirmationViewModel oldViewModel)
            {
                oldViewModel.RequestClose -= OnRequestClose;
            }

            if (e.NewValue is BookingConfirmationViewModel newViewModel)
            {
                newViewModel.RequestClose += OnRequestClose;
            }
        };
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        DialogResult = (sender as BookingConfirmationViewModel)?.IsSuccess;
        Close();
    }
}
