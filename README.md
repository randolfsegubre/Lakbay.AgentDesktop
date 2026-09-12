# Lakbay.AgentDesktop

A WPF desktop application for a Lakbay call-center agent: see who's calling
(a simulated telephony screen-pop over WCF), browse live accommodation/
package/activity availability for a destination, and book a package
directly for the caller over the phone.

Part of the [Lakbay](../Lakbay.Docs) platform — see
[ADR-0021](../Lakbay.Docs/docs/adr/ADR-0021-agent-channel-new-repos.md)
for why this exists and how it fits alongside `Lakbay.Web`, and this
repo's [CLAUDE.md](CLAUDE.md) for local setup and architecture notes.

## Stack

- **WPF (.NET 10) + MVVM**, with **Unity Container** as the composition
  root ([ADR-0022](../Lakbay.Docs/docs/adr/ADR-0022-wpf-unity-agent-desktop.md))
  — a deliberate choice, not the platform's usual
  `Microsoft.Extensions.DependencyInjection`.
- **WCF** ([ADR-0023](../Lakbay.Docs/docs/adr/ADR-0023-wcf-telephony-bridge.md))
  — a duplex `NetTcpBinding` service simulating legacy CTI/PBX screen-pop
  integration, the kind of integration real call-center desktop software
  still commonly does over WCF today.

## Status (2026-09-08)

Builds clean (`dotnet build Lakbay.AgentDesktop.slnx` — 0 warnings, 0
errors, including a fixed transitive `System.Security.Cryptography.Xml`
vulnerability). Verified live, all three processes running simultaneously:

1. `Lakbay.AgentDesktop.TelephonyBridge.Host` boots and listens on
   `net.tcp://localhost:8523/TelephonyBridge`.
2. The WPF app connects and subscribes — confirmed via the host's own
   console log (`Subscribed: session ... (1 active)`).
3. `Lakbay.AgentDesktop.TelephonyBridge.Simulator` triggers a fake call —
   the host log confirms the push was dispatched to the real, connected
   subscriber (`SimulateIncomingCall -> pushing to 1 subscriber(s)`).

The availability-browse and booking-confirmation flows work end-to-end
against `FakeAgentOpsClient`'s realistic sample data by default. The real
`AgentOpsHttpClient` was separately built and verified live against a real
running `Lakbay.AgentOps` instance (which itself proxies through to a real
`Lakbay.Booking`) — see `Lakbay.Docs`'s devlog for the exact verified
request/response pairs — but isn't yet the default registration in this
repo (see CLAUDE.md's "Switching to the real backend").

See [CLAUDE.md](CLAUDE.md)'s "Known gaps" section for what's honestly left:
no automated tests yet, accommodation pricing is a known simplification,
and the real-backend swap is a manual one-line change rather than the
shipped default.

## E2E testing note (2026-09-12)

This repo's own WPF UI was **not** re-verified live in this session's
platform-wide E2E pass — there's no headless/automated way to drive a
Windows desktop app the way a browser can be driven for `Lakbay.Web`. What
*was* re-verified live this session is the exact HTTP contract this app's
`AgentOpsHttpClient` calls: `POST /api/bookings/confirm` (now over gRPC on
`Lakbay.AgentOps`'s side, ADR-0027 — invisible to this app, no contract
change) and `GET /api/agent-offer/{destinationId}`, both confirmed working
against a real running `Lakbay.AgentOps` proxying to a real
`Lakbay.Booking`. See
[`../Lakbay.Docs/docs/05_DEVLOG.md`](../Lakbay.Docs/docs/05_DEVLOG.md)'s
2026-09-12 entry. If this app's own screen-pop/booking UI needs re-verifying
by actually looking at it, that has to happen on this machine directly, not
through an AI session.

## Path to production

- Swap `FakeAgentOpsClient` for `AgentOpsHttpClient` once `Lakbay.AgentOps`
  has a stable deployed URL.
- Replace the WCF `TelephonyBridge.Simulator` with a real PBX/CTI gateway
  integration — the `ITelephonyBridge` contract is designed to be the
  stable boundary that replacement happens behind.
- Add a real ClickOnce/MSIX packaging and deployment story for rolling
  this out to actual call-center desktops.
- Add the missing test project (ViewModels are already written to not
  need a UI thread, so this is mechanical, not a redesign).
