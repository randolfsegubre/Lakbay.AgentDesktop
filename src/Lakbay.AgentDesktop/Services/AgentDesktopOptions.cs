namespace Lakbay.AgentDesktop.Services;

/// <summary>
/// The one place the AgentOps base URL and TelephonyBridge address are
/// configured — everything else reads them from here instead of a literal
/// string, so there is exactly one place to change per environment.
/// Bound from appsettings.json's "AgentDesktop" section.
/// </summary>
public class AgentDesktopOptions
{
    public const string SectionName = "AgentDesktop";

    /// <summary>
    /// Base URL for Lakbay.AgentOps. Defaults to http://localhost:5250 if
    /// appsettings.json is missing or the key isn't set — see this repo's
    /// README for why this port was picked and how to change it.
    /// </summary>
    public string AgentOpsBaseUrl { get; set; } = "http://localhost:5250";

    /// <summary>
    /// net.tcp address of the TelephonyBridge WCF host. Must match
    /// TelephonyBridgeBinding.DefaultAddress unless both the host and this
    /// value are changed together.
    /// </summary>
    public string TelephonyBridgeAddress { get; set; } = "net.tcp://localhost:8523/TelephonyBridge";
}
