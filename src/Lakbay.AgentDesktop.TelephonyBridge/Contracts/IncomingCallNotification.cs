using System.Runtime.Serialization;

namespace Lakbay.AgentDesktop.TelephonyBridge.Contracts;

/// <summary>
/// Pushed from the TelephonyBridge service to every subscribed agent-desktop
/// client the moment a (simulated) call arrives — the "screen pop" payload.
/// </summary>
[DataContract]
public class IncomingCallNotification
{
    [DataMember]
    public Guid CallId { get; set; }

    [DataMember]
    public string CallerPhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Null means "unknown caller" — simulates a CRM lookup miss. Non-null
    /// simulates a match, so the agent's screen pop can show a name instead
    /// of a bare phone number.
    /// </summary>
    [DataMember]
    public string? MatchedCustomerName { get; set; }

    [DataMember]
    public DateTime CallStartUtc { get; set; }
}
