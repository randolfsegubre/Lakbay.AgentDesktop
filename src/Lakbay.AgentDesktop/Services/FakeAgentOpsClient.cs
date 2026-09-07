using Lakbay.AgentDesktop.Models;

namespace Lakbay.AgentDesktop.Services;

/// <summary>
/// Hardcoded, realistic sample data standing in for Lakbay.AgentOps while
/// that backend is built in parallel (ADR-0021). This is the current
/// default Unity registration (see App.xaml.cs) so the WPF app is fully
/// demoable without any other process running.
///
/// One product is deliberately wired to always fail confirmation
/// ("BOR-ACC-002", the Boracay overwater villa) so the "no longer
/// available" UI path — a real, expected outcome per the contract, not an
/// error — is exercised end to end without needing a live backend race
/// condition to reproduce it.
/// </summary>
public class FakeAgentOpsClient : IAgentOpsClient
{
    private const string AlwaysSoldOutProductId = "BOR-ACC-002";

    private static readonly Dictionary<string, AgentOfferDto> Offers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BOR"] = new AgentOfferDto
        {
            DestinationName = "Boracay",
            Accommodations =
            [
                new AccommodationDto { Id = "BOR-ACC-001", Name = "Shangri-La Boracay Resort & Spa", PricePerNight = 18500m },
                new AccommodationDto { Id = AlwaysSoldOutProductId, Name = "Discovery Shores Overwater Villa", PricePerNight = 24900m },
                new AccommodationDto { Id = "BOR-ACC-003", Name = "Henann Regency Resort & Spa", PricePerNight = 6800m },
            ],
            Packages =
            [
                new PackageDto { Id = "BOR-PKG-001", Name = "3D2N Boracay Island Escape (Flight + Hotel)", Price = 12500m },
                new PackageDto { Id = "BOR-PKG-002", Name = "5D4N Boracay Beach & Island Hopping", Price = 21900m },
            ],
            Activities =
            [
                new ActivityDto { Id = "BOR-ACT-001", Name = "Island Hopping Tour", Price = 1200m },
                new ActivityDto { Id = "BOR-ACT-002", Name = "Parasailing", Price = 2500m },
                new ActivityDto { Id = "BOR-ACT-003", Name = "Sunset Sailing Cruise", Price = 1800m },
            ],
        },
        ["PLW"] = new AgentOfferDto
        {
            DestinationName = "Palawan (El Nido)",
            Accommodations =
            [
                new AccommodationDto { Id = "PLW-ACC-001", Name = "El Nido Resorts Lagen Island", PricePerNight = 32000m },
                new AccommodationDto { Id = "PLW-ACC-002", Name = "Nay Palad Hideaway", PricePerNight = 45000m },
            ],
            Packages =
            [
                new PackageDto { Id = "PLW-PKG-001", Name = "4D3N El Nido Island Escapade", Price = 19800m },
            ],
            Activities =
            [
                new ActivityDto { Id = "PLW-ACT-001", Name = "El Nido Tour A (Big Lagoon, Small Lagoon)", Price = 1400m },
                new ActivityDto { Id = "PLW-ACT-002", Name = "El Nido Tour C (Hidden Beaches)", Price = 1600m },
            ],
        },
        ["BAG"] = new AgentOfferDto
        {
            DestinationName = "Baguio",
            Accommodations =
            [
                new AccommodationDto { Id = "BAG-ACC-001", Name = "The Manor at Camp John Hay", PricePerNight = 9200m },
                new AccommodationDto { Id = "BAG-ACC-002", Name = "Baguio Country Club", PricePerNight = 7500m },
            ],
            Packages =
            [
                new PackageDto { Id = "BAG-PKG-001", Name = "2D1N Baguio Highlands Getaway", Price = 6800m },
            ],
            Activities =
            [
                new ActivityDto { Id = "BAG-ACT-001", Name = "Strawberry Farm & Mines View Tour", Price = 900m },
            ],
        },
    };

    public async Task<AgentOfferDto> GetOfferAsync(string destinationCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationCode);

        // A small realistic delay so the UI's loading state is actually
        // exercised, same as it would be against a real network call.
        await Task.Delay(300, cancellationToken).ConfigureAwait(false);

        if (Offers.TryGetValue(destinationCode.Trim(), out var offer))
        {
            return offer;
        }

        throw new KeyNotFoundException(
            $"No fake offer configured for destination code '{destinationCode}'. " +
            $"Known codes: {string.Join(", ", Offers.Keys)}.");
    }

    public async Task<BookingResultDto> ConfirmBookingAsync(ConfirmBookingRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await Task.Delay(400, cancellationToken).ConfigureAwait(false);

        if (string.Equals(request.ProductId, AlwaysSoldOutProductId, StringComparison.OrdinalIgnoreCase))
        {
            return new BookingResultDto
            {
                Success = false,
                BookingId = null,
                PaymentStatus = null,
                Message = "This offer is no longer available — it was booked by another channel moments ago. " +
                           "Please choose a different accommodation for the customer.",
            };
        }

        return new BookingResultDto
        {
            Success = true,
            BookingId = $"LKB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            PaymentStatus = "AuthorizedOverPhone",
            Message = "Booking confirmed. A confirmation email/SMS will follow shortly.",
        };
    }
}
