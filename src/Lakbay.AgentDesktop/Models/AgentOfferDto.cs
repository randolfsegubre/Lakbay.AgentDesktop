using System.Text.Json.Serialization;

namespace Lakbay.AgentDesktop.Models;

/// <summary>
/// The agent-shaped "offer" for one destination: GET /api/agent-offer/{destinationCode}.
/// Matches the contract documented in ADR-0021 (aggregated catalog + availability
/// + package data bundled for a live phone call).
/// </summary>
public class AgentOfferDto
{
    [JsonPropertyName("destinationName")]
    public string DestinationName { get; set; } = string.Empty;

    [JsonPropertyName("accommodations")]
    public List<AccommodationDto> Accommodations { get; set; } = new();

    [JsonPropertyName("packages")]
    public List<PackageDto> Packages { get; set; } = new();

    [JsonPropertyName("activities")]
    public List<ActivityDto> Activities { get; set; } = new();
}

public class AccommodationDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("pricePerNight")]
    public decimal PricePerNight { get; set; }
}

public class PackageDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}

public class ActivityDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
