using System.Text.Json.Serialization;

namespace Lakbay.AgentDesktop.Models;

/// <summary>
/// POST /api/bookings/confirm request body. Channel is always "Agent" for
/// this app — that's the whole point of a distinct agent channel
/// (ADR-0021/ADR-0026): the same booking domain, reached a second way.
/// </summary>
public class ConfirmBookingRequest
{
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("dateSlot")]
    public DateOnly DateSlot { get; set; }

    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = "Agent";
}

/// <summary>
/// POST /api/bookings/confirm response. A false <see cref="Success"/> is a
/// normal, expected outcome (e.g. "no longer available") — the caller must
/// show <see cref="Message"/> to the agent, not treat this as a transport
/// error.
/// </summary>
public class BookingResultDto
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("bookingId")]
    public string? BookingId { get; set; }

    [JsonPropertyName("paymentStatus")]
    public string? PaymentStatus { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
