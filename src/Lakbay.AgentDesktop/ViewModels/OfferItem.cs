namespace Lakbay.AgentDesktop.ViewModels;

public enum OfferItemKind
{
    Accommodation,
    Package,
    Activity,
}

/// <summary>
/// One row in the availability-browse panel — an accommodation, package, or
/// activity flattened to the same shape so all three can be selected and
/// booked through the same booking-confirmation flow.
/// </summary>
public class OfferItem
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required OfferItemKind Kind { get; init; }

    public string PriceDisplay => Kind == OfferItemKind.Accommodation
        ? $"PHP {Price:N0} / night"
        : $"PHP {Price:N0}";

    public string KindDisplay => Kind switch
    {
        OfferItemKind.Accommodation => "Accommodation",
        OfferItemKind.Package => "Package",
        OfferItemKind.Activity => "Activity",
        _ => Kind.ToString(),
    };
}
