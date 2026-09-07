using System.Windows.Input;
using Lakbay.AgentDesktop.Models;
using Lakbay.AgentDesktop.Services;

namespace Lakbay.AgentDesktop.ViewModels;

/// <summary>
/// Backs the booking-confirmation dialog: confirms a booking for the
/// selected offer item against the current caller. Raises
/// <see cref="RequestClose"/> (never touches a <c>Window</c> directly) so
/// the dialog's own code-behind — the one place allowed to hold a
/// <c>Window</c> reference — can close itself (ADR-0022).
/// </summary>
public class BookingConfirmationViewModel : ViewModelBase
{
    private readonly IAgentOpsClient _agentOpsClient;

    private string? _customerId;
    private DateOnly _dateSlot = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
    private bool _isSubmitting;
    private bool? _isSuccess;
    private string? _resultMessage;
    private string? _bookingId;

    public BookingConfirmationViewModel(IAgentOpsClient agentOpsClient, OfferItem item, string? callerCustomerId)
    {
        _agentOpsClient = agentOpsClient ?? throw new ArgumentNullException(nameof(agentOpsClient));
        Item = item ?? throw new ArgumentNullException(nameof(item));
        _customerId = callerCustomerId;

        ConfirmCommand = new RelayCommand(ConfirmAsync, () => !IsSubmitting && IsSuccess != true);
        CloseCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));
    }

    public event EventHandler? RequestClose;

    public OfferItem Item { get; }

    public string? CustomerId
    {
        get => _customerId;
        set => SetProperty(ref _customerId, value);
    }

    public DateOnly DateSlot
    {
        get => _dateSlot;
        set => SetProperty(ref _dateSlot, value);
    }

    public bool IsSubmitting
    {
        get => _isSubmitting;
        private set => SetProperty(ref _isSubmitting, value);
    }

    /// <summary>Null = not yet submitted. True/false = the AgentOps response.</summary>
    public bool? IsSuccess
    {
        get => _isSuccess;
        private set => SetProperty(ref _isSuccess, value);
    }

    public string? ResultMessage
    {
        get => _resultMessage;
        private set => SetProperty(ref _resultMessage, value);
    }

    public string? BookingId
    {
        get => _bookingId;
        private set => SetProperty(ref _bookingId, value);
    }

    public ICommand ConfirmCommand { get; }

    public ICommand CloseCommand { get; }

    private async Task ConfirmAsync()
    {
        IsSubmitting = true;
        ResultMessage = null;

        try
        {
            var request = new ConfirmBookingRequest
            {
                ProductId = Item.Id,
                DateSlot = DateSlot,
                CustomerId = CustomerId,
                Channel = "Agent",
            };

            var result = await _agentOpsClient.ConfirmBookingAsync(request).ConfigureAwait(true);

            // Success == false is a normal, expected business outcome (e.g.
            // "no longer available") — surfaced to the agent here, never
            // thrown as an exception or mistaken for a transport failure.
            IsSuccess = result.Success;
            ResultMessage = result.Message;
            BookingId = result.BookingId;
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            ResultMessage = $"Could not reach Lakbay.AgentOps: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
