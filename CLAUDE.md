# Lakbay.AgentDesktop — Start Here

This file is intentionally short. It exists so any Claude Code session (or
other AI coding assistant) rooted here auto-loads it and is pointed at the
real documentation before touching anything.

**Read, in this order, before writing any code:**

1. [../Lakbay.Docs/docs/01_CLAUDE.md](../Lakbay.Docs/docs/01_CLAUDE.md) —
   the platform AI operating manual.
2. [../Lakbay.Docs/docs/02_BUILD_PLAN.md](../Lakbay.Docs/docs/02_BUILD_PLAN.md)
   — **Phase 7 (Agent Channel)** is this repo's phase.
3. [../Lakbay.Docs/docs/04_TASKS.md](../Lakbay.Docs/docs/04_TASKS.md) —
   current status across the whole platform.
4. Decisions this repo must honor:
   [ADR-0021](../Lakbay.Docs/docs/adr/ADR-0021-agent-channel-new-repos.md)
   (why this repo exists, and its relationship to `Lakbay.AgentOps`),
   [ADR-0022](../Lakbay.Docs/docs/adr/ADR-0022-wpf-unity-agent-desktop.md)
   (WPF/MVVM + Unity as the composition root — not
   `Microsoft.Extensions.DependencyInjection`), and
   [ADR-0023](../Lakbay.Docs/docs/adr/ADR-0023-wcf-telephony-bridge.md)
   (the WCF duplex screen-pop simulation).

## What this repo is

A WPF desktop app for a call-center agent: see who's calling (screen-pop via
the `Lakbay.AgentDesktop.TelephonyBridge` WCF service), browse live
accommodation/package/activity availability, and book a package directly
for the caller. The platform's **second front-end client**, alongside
`Lakbay.Web` — same backend concept (a headless service, ADR-0006), a
completely different presentation technology.

**Never calls `Lakbay.AgentOps`/`Lakbay.Booking`'s databases directly** —
everything goes through `IAgentOpsClient` (real HTTP or `FakeAgentOpsClient`,
see below).

**Only Packages are bookable.** In the real Lakbay schema, a "Package" here
is a `Product` — the only entity `Lakbay.Booking`'s `ConfirmBookingCommand`
actually knows how to book (`ProductId` + `DateSlot`). Accommodations and
Activities inform what the agent says on the call; they carry no bookable
id of their own.

## Local setup

```powershell
# Terminal 1 - the simulated CTI/PBX gateway (net48, see ADR-0023 for why)
cd src/Lakbay.AgentDesktop.TelephonyBridge.Host
dotnet run --framework net48

# Terminal 2 - the WPF app itself
cd src/Lakbay.AgentDesktop
dotnet run
```

The app defaults to `FakeAgentOpsClient` (hardcoded sample data for BOR/PLW/BAG),
so it's fully demoable with just the two commands above — no `Lakbay.AgentOps`
or `Lakbay.Booking` required.

**Trigger a simulated incoming call** (Terminal 3, while the app is running):

```powershell
cd src/Lakbay.AgentDesktop.TelephonyBridge.Simulator
dotnet run -- +639171234567 "Juan Dela Cruz"
```

Watch Terminal 1's console for `Subscribed`/`SimulateIncomingCall -> pushing
to N subscriber(s)` — confirms the duplex push actually reached the desktop
app, independent of anything visible in the WPF window itself.

**Switching to the real `Lakbay.AgentOps` backend**: in `App.xaml.cs`, swap
the Unity registration from `FakeAgentOpsClient` to `AgentOpsHttpClient`
(one line, commented right next to it). Requires `Lakbay.AgentOps` running
on the URL configured in `appsettings.json`'s `AgentDesktop:AgentOpsBaseUrl`
(defaults to `http://localhost:5250`).

## WCF hosting note (read before changing the TelephonyBridge binding)

`Lakbay.AgentDesktop.TelephonyBridge` multi-targets `net48;net10.0` on
purpose: CoreWCF's `NetTcpBinding` does not support duplex/callback
contracts as of the version available when this was built, so the **host**
(`Lakbay.AgentDesktop.TelephonyBridge.Host`) runs on classic .NET Framework,
which has full `System.ServiceModel` duplex support. The WPF app and the
Simulator (both WCF *clients*, not hosts) run on modern .NET via
`System.ServiceModel.NetTcp`/`Primitives`, which fully supports the client
side of a duplex contract. If CoreWCF adds duplex support later, revisit
this split — but verify it live (a real host boot + a real push received)
before removing the net48 target, not just because a NuGet package version
bumped.

## Known gaps (as of 2026-09-08)

- `AgentOpsHttpClient` is implemented against `Lakbay.AgentOps`'s real,
  documented contract and was exercised live against a real running
  `Lakbay.AgentOps` instance (proxying through to a real `Lakbay.Booking`)
  during this repo's build — see `Lakbay.Docs`'s devlog for the verified
  request/response pairs. It is not yet the *default* Unity registration
  in this repo's own `App.xaml.cs` (see "Switching to the real backend"
  above) — that's a one-line change left for whoever wires up an
  environment where `Lakbay.AgentOps` runs continuously.
- `MainViewModel`'s call-to-booking correlation is incomplete: a booking
  confirmed while a call is active passes the caller's `CallId` as
  `CustomerId`, but there's no real customer-identity system anywhere in
  this platform yet — this is a placeholder linking mechanism, not a real
  CRM lookup.
- No automated tests yet. ViewModels are written to be unit-testable
  (no WPF types, no UI thread dependency) but no test project exists —
  a real, near-zero-friction gap to close next given the design already
  supports it.
- Accommodations show no per-night price in the UI (`PriceDisplay` reads
  "PHP 0 / night") — the real Lakbay schema has no direct price on
  `Accommodation` (pricing lives on `RoomType`/`PriceBand`, or on the
  `Product`/Package level), and this repo doesn't fetch room-type pricing
  per accommodation to keep the offer lookup to one round trip. A known,
  deliberate simplification, not a bug.
