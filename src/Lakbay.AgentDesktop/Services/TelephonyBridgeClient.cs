using System.ServiceModel;
using System.Windows;
using Lakbay.AgentDesktop.TelephonyBridge;
using Lakbay.AgentDesktop.TelephonyBridge.Contracts;

namespace Lakbay.AgentDesktop.Services;

/// <summary>
/// Real duplex WCF client to the TelephonyBridge host. Implements the
/// callback contract itself (<see cref="ITelephonyBridgeCallback"/>) so the
/// service can push <see cref="OnIncomingCall"/> straight into this
/// instance, which then re-raises it as a plain .NET event on the UI
/// thread for the ViewModel to consume.
/// </summary>
public class TelephonyBridgeClient : ITelephonyBridgeClient, ITelephonyBridgeCallback
{
    private readonly AgentDesktopOptions _options;
    private DuplexChannelFactory<ITelephonyBridge>? _factory;
    private ITelephonyBridge? _channel;

    public event EventHandler<IncomingCallNotification>? IncomingCallReceived;

    public TelephonyBridgeClient(AgentDesktopOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task ConnectAsync()
    {
        _factory = new DuplexChannelFactory<ITelephonyBridge>(
            new InstanceContext(this),
            TelephonyBridgeBinding.Create(),
            new EndpointAddress(_options.TelephonyBridgeAddress));

        _channel = _factory.CreateChannel();
        ((IContextChannel)_channel).Open();
        _channel.Subscribe();

        return Task.CompletedTask;
    }

    public void EndCall(Guid callId) => _channel?.EndCall(callId);

    public void OnIncomingCall(IncomingCallNotification notification)
    {
        // This runs on a WCF-owned callback thread, not the UI thread —
        // marshal before touching anything bound to the UI.
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            IncomingCallReceived?.Invoke(this, notification);
        }
        else
        {
            dispatcher.Invoke(() => IncomingCallReceived?.Invoke(this, notification));
        }
    }

    public void Dispose()
    {
        try
        {
            if (_channel is IContextChannel { State: CommunicationState.Opened } channel)
            {
                channel.Close();
            }
            else
            {
                (_channel as ICommunicationObject)?.Abort();
            }

            _factory?.Close();
        }
        catch
        {
            (_channel as ICommunicationObject)?.Abort();
            _factory?.Abort();
        }
    }
}
