using System.ServiceModel;
using Lakbay.AgentDesktop.TelephonyBridge;
using Lakbay.AgentDesktop.TelephonyBridge.Contracts;

if (args.Length < 1)
{
    Console.WriteLine("Lakbay.AgentDesktop.TelephonyBridge.Simulator");
    Console.WriteLine();
    Console.WriteLine("Stands in for a real PBX: connects to the TelephonyBridge host and");
    Console.WriteLine("triggers a fake incoming call, pushed to every subscribed agent desktop.");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  Lakbay.AgentDesktop.TelephonyBridge.Simulator <phoneNumber> [matchedCustomerName]");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  Lakbay.AgentDesktop.TelephonyBridge.Simulator +639171234567");
    Console.WriteLine("  Lakbay.AgentDesktop.TelephonyBridge.Simulator +639171234567 \"Juan Dela Cruz\"");
    return 1;
}

var phoneNumber = args[0];
var matchedCustomerName = args.Length > 1 ? args[1] : null;

var callbackHandler = new NoOpTelephonyBridgeCallback();
var factory = new DuplexChannelFactory<ITelephonyBridge>(
    new InstanceContext(callbackHandler),
    TelephonyBridgeBinding.Create(),
    new EndpointAddress(TelephonyBridgeBinding.DefaultAddress));

Console.WriteLine($"[Simulator] Connecting to {TelephonyBridgeBinding.DefaultAddress} ...");
var channel = factory.CreateChannel();

try
{
    ((IContextChannel)channel).Open();
    Console.WriteLine("[Simulator] Connected.");
    Console.WriteLine(
        $"[Simulator] Triggering incoming call from {phoneNumber} " +
        $"({(matchedCustomerName is null ? "unknown caller" : matchedCustomerName)}) ...");

    channel.SimulateIncomingCall(phoneNumber, matchedCustomerName);

    Console.WriteLine("[Simulator] Call triggered. Check the agent desktop / host console for the push.");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"[Simulator] Failed: {ex}");
    return 1;
}
finally
{
    try
    {
        ((IContextChannel)channel).Close();
        factory.Close();
    }
    catch
    {
        factory.Abort();
    }
}

// This client never receives a call, so every callback member is a no-op —
// it still has to implement the contract to satisfy the duplex handshake.
internal sealed class NoOpTelephonyBridgeCallback : ITelephonyBridgeCallback
{
    public void OnIncomingCall(IncomingCallNotification notification)
    {
        // Not expected: the simulator is a caller, not a listener.
    }
}
