using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Lakbay.AgentDesktop.Models;

namespace Lakbay.AgentDesktop.Services;

/// <summary>
/// Real HTTP implementation of <see cref="IAgentOpsClient"/>, talking to
/// Lakbay.AgentOps. Not yet exercised against a live server — see this
/// repo's README "known gaps" section. Implemented against the documented
/// contract (ADR-0021) so the swap from <see cref="FakeAgentOpsClient"/> is
/// a one-line Unity registration change, not a rewrite, once that backend
/// exists.
/// </summary>
public class AgentOpsHttpClient : IAgentOpsClient, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;

    public AgentOpsHttpClient(AgentDesktopOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(options.AgentOpsBaseUrl, UriKind.Absolute),
        };
    }

    public async Task<AgentOfferDto> GetOfferAsync(string destinationCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationCode);

        var response = await _httpClient
            .GetAsync($"api/agent-offer/{Uri.EscapeDataString(destinationCode)}", cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var offer = await response.Content
            .ReadFromJsonAsync<AgentOfferDto>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return offer ?? throw new InvalidOperationException(
            $"Lakbay.AgentOps returned an empty body for destination '{destinationCode}'.");
    }

    public async Task<BookingResultDto> ConfirmBookingAsync(ConfirmBookingRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient
            .PostAsJsonAsync("api/bookings/confirm", request, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        // A booking that failed for a business reason (e.g. no longer
        // available) is still expected to come back as a 2xx with
        // Success == false per the documented contract — only a genuine
        // transport/server error should throw here.
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<BookingResultDto>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return result ?? throw new InvalidOperationException(
            "Lakbay.AgentOps returned an empty body for a booking confirmation.");
    }

    public void Dispose() => _httpClient.Dispose();
}
