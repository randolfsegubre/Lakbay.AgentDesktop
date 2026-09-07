using System.ServiceModel;

namespace Lakbay.AgentDesktop.TelephonyBridge.Contracts;

/// <summary>
/// The callback contract a subscribed agent-desktop client implements so the
/// TelephonyBridge service can push notifications to it. This is the
/// "screen pop" push channel — the client never polls for it.
/// </summary>
[ServiceContract]
public interface ITelephonyBridgeCallback
{
    [OperationContract(IsOneWay = true)]
    void OnIncomingCall(IncomingCallNotification notification);
}
