using Lakbay.AgentDesktop.TelephonyBridge.Contracts;

namespace Lakbay.AgentDesktop.Services;

/// <summary>
/// The WPF-side wrapper around the duplex WCF client to
/// Lakbay.AgentDesktop.TelephonyBridge.Host (ADR-0023). ViewModels depend
/// on this abstraction, never on <c>DuplexChannelFactory</c>/<c>ITelephonyBridge</c>
/// directly, so they stay testable with a fake that raises
/// <see cref="IncomingCallReceived"/> synchronously.
/// </summary>
public interface ITelephonyBridgeClient : IDisposable
{
    /// <summary>
    /// Raised whenever the bridge pushes a new incoming call. The
    /// implementation is responsible for marshaling this onto the UI
    /// thread — subscribers can update bound properties directly.
    /// </summary>
    event EventHandler<IncomingCallNotification>? IncomingCallReceived;

    /// <summary>Opens the duplex channel and subscribes for push notifications.</summary>
    Task ConnectAsync();

    /// <summary>Tells the bridge the agent ended the call.</summary>
    void EndCall(Guid callId);
}
