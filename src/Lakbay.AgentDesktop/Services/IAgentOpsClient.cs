using Lakbay.AgentDesktop.Models;

namespace Lakbay.AgentDesktop.Services;

/// <summary>
/// Client-side contract for Lakbay.AgentOps (ADR-0021) — the agent-shaped
/// aggregation of catalog + availability + package data, plus booking
/// confirmation. Two implementations exist: <see cref="AgentOpsHttpClient"/>
/// (real HTTP) and <see cref="FakeAgentOpsClient"/> (hardcoded sample data,
/// the current default Unity registration — see App.xaml.cs and this repo's
/// README for how to swap to the real one).
/// </summary>
public interface IAgentOpsClient
{
    /// <summary>GET /api/agent-offer/{destinationCode}</summary>
    Task<AgentOfferDto> GetOfferAsync(string destinationCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// POST /api/bookings/confirm. A <see cref="BookingResultDto"/> with
    /// <c>Success == false</c> (e.g. the offer went unavailable between
    /// browse and confirm) is a normal return value, not an exception —
    /// callers must branch on it and show <c>Message</c> to the agent.
    /// </summary>
    Task<BookingResultDto> ConfirmBookingAsync(ConfirmBookingRequest request, CancellationToken cancellationToken = default);
}
