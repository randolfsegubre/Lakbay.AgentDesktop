using System;
using System.ServiceModel;
using System.Threading;
using Lakbay.AgentDesktop.TelephonyBridge;
using Lakbay.AgentDesktop.TelephonyBridge.Contracts;

namespace Lakbay.AgentDesktop.TelephonyBridge.Host
{
    /// <summary>
    /// Console host for the simulated CTI/PBX gateway (ADR-0023). Run this,
    /// then run Lakbay.AgentDesktop.TelephonyBridge.Simulator (or the WPF
    /// app's own "Simulate incoming call" flow) against it.
    /// </summary>
    internal static class Program
    {
        private static void Main()
        {
            var address = new Uri(TelephonyBridgeBinding.DefaultAddress);
            var service = new TelephonyBridgeService();

            using (var host = new ServiceHost(service, address))
            {
                host.AddServiceEndpoint(typeof(ITelephonyBridge), TelephonyBridgeBinding.Create(), string.Empty);

                host.Opened += (_, _) => Console.WriteLine($"[Host] TelephonyBridge listening at {address}");
                host.Faulted += (_, _) => Console.WriteLine("[Host] Service host faulted.");

                try
                {
                    host.Open();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Host] Failed to open: {ex}");
                    return;
                }

                Console.WriteLine("Lakbay.AgentDesktop.TelephonyBridge.Host");
                Console.WriteLine("Simulated CTI/PBX gateway — press Enter (or Ctrl+C) to stop.");
                Console.WriteLine();

                // Console.ReadLine() returns immediately with null when
                // stdin is redirected/closed (e.g. run as a background
                // process) — fall back to waiting on Ctrl+C so the host
                // doesn't exit the instant it starts in that case.
                var shutdown = new ManualResetEventSlim(false);
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    shutdown.Set();
                };

                if (!Console.IsInputRedirected)
                {
                    Console.ReadLine();
                }
                else
                {
                    shutdown.Wait();
                }

                host.Close();
            }
        }
    }
}
