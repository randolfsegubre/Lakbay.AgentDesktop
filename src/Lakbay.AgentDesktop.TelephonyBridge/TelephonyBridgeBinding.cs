using System.ServiceModel;

namespace Lakbay.AgentDesktop.TelephonyBridge;

/// <summary>
/// Single source of truth for the NetTcpBinding + security settings, shared
/// by the host, the WPF client wrapper, and the simulator so the three can
/// never silently drift apart on a binding detail (a classic source of "it
/// compiles but the handshake fails" WCF bugs).
///
/// Security: <see cref="SecurityMode.Transport"/> with
/// <see cref="TcpClientCredentialType.Windows"/> — this is deliberately NOT
/// <see cref="SecurityMode.None"/>. It authenticates the connection using
/// the caller's Windows identity via SSPI/Negotiate over the TCP transport,
/// which needs no certificate to provision for a local/on-prem demo (unlike
/// a certificate-based Transport-with-TLS setup) while still being a real,
/// non-trivial security posture — a reasonable stand-in for how an on-prem
/// CTI gateway on the same domain/LAN would actually be secured. See this
/// repo's README "Security posture" section for the full rationale.
/// </summary>
public static class TelephonyBridgeBinding
{
    public const string DefaultAddress = "net.tcp://localhost:8523/TelephonyBridge";

    public static NetTcpBinding Create()
    {
        var binding = new NetTcpBinding(SecurityMode.Transport)
        {
            Security =
            {
                Mode = SecurityMode.Transport,
                Transport = { ClientCredentialType = TcpClientCredentialType.Windows },
            },
        };
        return binding;
    }
}
