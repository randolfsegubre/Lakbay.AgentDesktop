using System.Windows.Input;
using Lakbay.AgentDesktop.Services;
using Lakbay.AgentDesktop.TelephonyBridge.Contracts;

namespace Lakbay.AgentDesktop.ViewModels;

/// <summary>
/// Composes the three panels (incoming call, availability browse, booking) and owns the
/// TelephonyBridge connection - the only ViewModel that talks to
/// <see cref="ITelephonyBridgeClient"/> directly, since it's the one that needs to react
/// to a call arriving regardless of which panel the agent is currently looking at.
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly IAgentOpsClient _agentOpsClient;
    private readonly ITelephonyBridgeClient _telephonyBridgeClient;
    private readonly INavigationService _navigationService;

    public MainViewModel(
        IAgentOpsClient agentOpsClient,
        ITelephonyBridgeClient telephonyBridgeClient,
        INavigationService navigationService,
        IncomingCallViewModel incomingCall,
        AvailabilityViewModel availability)
    {
        _agentOpsClient = agentOpsClient ?? throw new ArgumentNullException(nameof(agentOpsClient));
        _telephonyBridgeClient = telephonyBridgeClient ?? throw new ArgumentNullException(nameof(telephonyBridgeClient));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        IncomingCall = incomingCall;
        Availability = availability;

        _telephonyBridgeClient.IncomingCallReceived += OnIncomingCallReceived;

        BookSelectedItemCommand = new RelayCommand(
            BookSelectedItemAsync,
            // Only Packages carry a bookable ProductId in Lakbay.Booking's domain -
            // Accommodations/Activities are shown for context but aren't booking targets.
            () => Availability.SelectedItem?.Kind == OfferItemKind.Package);

        EndCallCommand = new RelayCommand(EndCall, () => IncomingCall.HasActiveCall);
    }

    public IncomingCallViewModel IncomingCall { get; }

    public AvailabilityViewModel Availability { get; }

    public ICommand BookSelectedItemCommand { get; }

    public ICommand EndCallCommand { get; }

    public string BridgeStatus { get; private set; } = "Connecting to TelephonyBridge...";

    public async Task ConnectTelephonyBridgeAsync()
    {
        try
        {
            await _telephonyBridgeClient.ConnectAsync().ConfigureAwait(true);
            BridgeStatus = "TelephonyBridge connected - waiting for a call.";
        }
        catch (Exception ex)
        {
            BridgeStatus = $"TelephonyBridge unavailable: {ex.Message}. " +
                            "Run Lakbay.AgentDesktop.TelephonyBridge.Host first (see README).";
        }

        OnPropertyChanged(nameof(BridgeStatus));
    }

    private void OnIncomingCallReceived(object? sender, IncomingCallNotification notification)
    {
        IncomingCall.Apply(notification);
        OnPropertyChanged(nameof(IncomingCall));
    }

    private Task BookSelectedItemAsync()
    {
        var item = Availability.SelectedItem;
        if (item is null || item.Kind != OfferItemKind.Package)
        {
            // Only Packages are bookable (Lakbay.Booking's ConfirmBookingCommand takes a
            // ProductId, and a "Product" is exactly what a Package represents in the real
            // Lakbay schema) - Accommodations/Activities inform the call, not book directly.
            return Task.CompletedTask;
        }

        var callerCustomerId = IncomingCall.HasActiveCall ? IncomingCall.CallId.ToString() : null;
        var dialogViewModel = new BookingConfirmationViewModel(_agentOpsClient, item, callerCustomerId);
        _navigationService.ShowDialog(dialogViewModel);

        return Task.CompletedTask;
    }

    private void EndCall()
    {
        if (IncomingCall.HasActiveCall)
        {
            _telephonyBridgeClient.EndCall(IncomingCall.CallId);
        }

        IncomingCall.Clear();
    }

    public void Dispose()
    {
        _telephonyBridgeClient.IncomingCallReceived -= OnIncomingCallReceived;
        _telephonyBridgeClient.Dispose();
    }
}
