using System.ServiceModel;

namespace Lakbay.AgentDesktop.TelephonyBridge.Contracts;

/// <summary>
/// Simulates the legacy CTI/PBX gateway contract a real call-center desktop
/// would integrate with (ADR-0023). Duplex: the agent desktop calls
/// <see cref="Subscribe"/> once and then receives pushed
/// <see cref="ITelephonyBridgeCallback.OnIncomingCall"/> notifications for as
/// long as the session/connection stays open.
/// </summary>
[ServiceContract(CallbackContract = typeof(ITelephonyBridgeCallback), SessionMode = SessionMode.Required)]
public interface ITelephonyBridge
{
    /// <summary>
    /// Registers the calling client for push notifications. The call
    /// returns immediately; notifications arrive later on the duplex
    /// callback channel, for the lifetime of this session.
    /// </summary>
    [OperationContract]
    void Subscribe();

    /// <summary>
    /// Logs call end and closes out the active call context. Used by the
    /// agent desktop when the agent hangs up / wraps up the call.
    /// </summary>
    [OperationContract(IsOneWay = true)]
    void EndCall(Guid callId);

    /// <summary>
    /// Dev/test-only hook standing in for a real PBX trigger — this is
    /// exactly what a genuine telephony gateway event would replace
    /// (ADR-0023's stated replacement boundary). Invoked by
    /// Lakbay.AgentDesktop.TelephonyBridge.Simulator to push a fake
    /// incoming call to every currently-subscribed client.
    /// </summary>
    [OperationContract(IsOneWay = true)]
    void SimulateIncomingCall(string callerPhoneNumber, string? matchedCustomerName);
}
