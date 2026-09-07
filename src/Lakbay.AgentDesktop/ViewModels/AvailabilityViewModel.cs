using System.Collections.ObjectModel;
using System.Windows.Input;
using Lakbay.AgentDesktop.Services;

namespace Lakbay.AgentDesktop.ViewModels;

/// <summary>
/// Backs the availability-browse panel: agent types/picks a destination
/// code, this calls <see cref="IAgentOpsClient.GetOfferAsync"/> and exposes
/// the accommodations/packages/activities as one flat, selectable list.
/// </summary>
public class AvailabilityViewModel : ViewModelBase
{
    private readonly IAgentOpsClient _agentOpsClient;

    private string _destinationCode = "BOR";
    private string? _destinationName;
    private bool _isLoading;
    private string? _errorMessage;
    private OfferItem? _selectedItem;

    public AvailabilityViewModel(IAgentOpsClient agentOpsClient)
    {
        _agentOpsClient = agentOpsClient ?? throw new ArgumentNullException(nameof(agentOpsClient));
        LoadOfferCommand = new RelayCommand(LoadOfferAsync);
    }

    public ObservableCollection<OfferItem> Items { get; } = new();

    public string DestinationCode
    {
        get => _destinationCode;
        set => SetProperty(ref _destinationCode, value);
    }

    public string? DestinationName
    {
        get => _destinationName;
        private set => SetProperty(ref _destinationName, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public OfferItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public ICommand LoadOfferCommand { get; }

    private async Task LoadOfferAsync()
    {
        if (string.IsNullOrWhiteSpace(DestinationCode))
        {
            ErrorMessage = "Enter a destination code (e.g. BOR, PLW, BAG).";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        SelectedItem = null;

        try
        {
            var offer = await _agentOpsClient.GetOfferAsync(DestinationCode.Trim()).ConfigureAwait(true);

            Items.Clear();
            DestinationName = offer.DestinationName;

            foreach (var accommodation in offer.Accommodations)
            {
                Items.Add(new OfferItem
                {
                    Id = accommodation.Id,
                    Name = accommodation.Name,
                    Price = accommodation.PricePerNight,
                    Kind = OfferItemKind.Accommodation,
                });
            }

            foreach (var package in offer.Packages)
            {
                Items.Add(new OfferItem
                {
                    Id = package.Id,
                    Name = package.Name,
                    Price = package.Price,
                    Kind = OfferItemKind.Package,
                });
            }

            foreach (var activity in offer.Activities)
            {
                Items.Add(new OfferItem
                {
                    Id = activity.Id,
                    Name = activity.Name,
                    Price = activity.Price,
                    Kind = OfferItemKind.Activity,
                });
            }
        }
        catch (Exception ex)
        {
            Items.Clear();
            DestinationName = null;
            ErrorMessage = $"Could not load availability: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
