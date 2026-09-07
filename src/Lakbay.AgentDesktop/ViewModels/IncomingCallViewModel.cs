using Lakbay.AgentDesktop.TelephonyBridge.Contracts;

namespace Lakbay.AgentDesktop.ViewModels;

/// <summary>
/// Backs the "incoming call" panel: empty/idle until the TelephonyBridge
/// pushes a notification, then shows the caller's phone number and (if
/// matched) name — the screen-pop itself.
/// </summary>
public class IncomingCallViewModel : ViewModelBase
{
    private bool _hasActiveCall;
    private Guid _callId;
    private string _callerPhoneNumber = string.Empty;
    private string? _matchedCustomerName;
    private DateTime _callStartUtc;

    public bool HasActiveCall
    {
        get => _hasActiveCall;
        private set => SetProperty(ref _hasActiveCall, value);
    }

    public Guid CallId
    {
        get => _callId;
        private set => SetProperty(ref _callId, value);
    }

    public string CallerPhoneNumber
    {
        get => _callerPhoneNumber;
        private set => SetProperty(ref _callerPhoneNumber, value);
    }

    public string? MatchedCustomerName
    {
        get => _matchedCustomerName;
        private set => SetProperty(ref _matchedCustomerName, value);
    }

    public bool IsKnownCaller => !string.IsNullOrWhiteSpace(MatchedCustomerName);

    public DateTime CallStartUtc
    {
        get => _callStartUtc;
        private set => SetProperty(ref _callStartUtc, value);
    }

    /// <summary>Display-ready greeting, e.g. "Incoming: Juan Dela Cruz" or "Incoming: unknown caller".</summary>
    public string DisplayGreeting => HasActiveCall
        ? $"Incoming call from {(IsKnownCaller ? MatchedCustomerName : "an unknown caller")} ({CallerPhoneNumber})"
        : "No active call";

    public void Apply(IncomingCallNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        CallId = notification.CallId;
        CallerPhoneNumber = notification.CallerPhoneNumber;
        MatchedCustomerName = notification.MatchedCustomerName;
        CallStartUtc = notification.CallStartUtc;
        HasActiveCall = true;

        OnPropertyChanged(nameof(IsKnownCaller));
        OnPropertyChanged(nameof(DisplayGreeting));
    }

    public void Clear()
    {
        HasActiveCall = false;
        CallId = Guid.Empty;
        CallerPhoneNumber = string.Empty;
        MatchedCustomerName = null;
        CallStartUtc = default;

        OnPropertyChanged(nameof(IsKnownCaller));
        OnPropertyChanged(nameof(DisplayGreeting));
    }
}
