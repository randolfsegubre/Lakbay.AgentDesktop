using System.Collections.Concurrent;
using System.ServiceModel;
using Lakbay.AgentDesktop.TelephonyBridge.Contracts;

namespace Lakbay.AgentDesktop.TelephonyBridge;

/// <summary>
/// Simulated CTI/PBX gateway. One shared instance (<see cref="InstanceContextMode.Single"/>)
/// tracks every subscribed agent-desktop callback channel so that
/// <see cref="SimulateIncomingCall"/> — called from a completely separate
/// connection by the Simulator — can push to all of them.
/// </summary>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public class TelephonyBridgeService : ITelephonyBridge
{
    // Keyed by WCF session id so a dropped connection can be identified and
    // pruned instead of accumulating dead callback channels forever.
    private readonly ConcurrentDictionary<string, ITelephonyBridgeCallback> _subscribers = new();

    public event Action<string>? SubscriberConnected;
    public event Action<string>? SubscriberDisconnected;

    public void Subscribe()
    {
        var sessionId = OperationContext.Current.SessionId
            ?? throw new InvalidOperationException("TelephonyBridge requires a sessionful binding.");

        var callback = OperationContext.Current.GetCallbackChannel<ITelephonyBridgeCallback>();
        _subscribers[sessionId] = callback;

        // Prune this subscriber the moment its channel goes away, so a
        // closed desktop app doesn't leave a stale entry that later throws
        // on push.
        var channel = (IContextChannel)callback;
        channel.Closed += (_, _) => Remove(sessionId);
        channel.Faulted += (_, _) => Remove(sessionId);

        SubscriberConnected?.Invoke(sessionId);
        Console.WriteLine($"[TelephonyBridge] Subscribed: session {sessionId} ({_subscribers.Count} active).");
    }

    public void EndCall(Guid callId)
    {
        Console.WriteLine($"[TelephonyBridge] EndCall: {callId} at {DateTime.UtcNow:O}.");
    }

    public void SimulateIncomingCall(string callerPhoneNumber, string? matchedCustomerName)
    {
        var notification = new IncomingCallNotification
        {
            CallId = Guid.NewGuid(),
            CallerPhoneNumber = callerPhoneNumber,
            MatchedCustomerName = matchedCustomerName,
            CallStartUtc = DateTime.UtcNow,
        };

        Console.WriteLine(
            $"[TelephonyBridge] SimulateIncomingCall: {callerPhoneNumber} " +
            $"({(matchedCustomerName is null ? "unknown caller" : matchedCustomerName)}) " +
            $"-> pushing to {_subscribers.Count} subscriber(s).");

        foreach (var subscriber in _subscribers)
        {
            var sessionId = subscriber.Key;
            var callback = subscriber.Value;
            try
            {
                callback.OnIncomingCall(notification);
            }
            catch (Exception ex)
            {
                // A dead channel throws here rather than via the Faulted
                // event in some cases (e.g. abrupt process kill) — prune it.
                Console.WriteLine($"[TelephonyBridge] Push to session {sessionId} failed ({ex.GetType().Name}); removing.");
                Remove(sessionId);
            }
        }
    }

    private void Remove(string sessionId)
    {
        if (_subscribers.TryRemove(sessionId, out _))
        {
            SubscriberDisconnected?.Invoke(sessionId);
            Console.WriteLine($"[TelephonyBridge] Unsubscribed: session {sessionId} ({_subscribers.Count} active).");
        }
    }
}
