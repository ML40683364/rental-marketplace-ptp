using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly ILocationService _locationService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Item> nearbyItems = new();

    [ObservableProperty]
    private double radiusKm = 10;

    [ObservableProperty]
    private string currentLocationText = "Fetching location...";

    public NearbyItemsViewModel(IRentalService rentalService, ILocationService locationService, INavigationService navigationService)
    {
        _rentalService = rentalService;
        _locationService = locationService;
        _navigationService = navigationService;
        Title = "Nearby Items";
    }

    [RelayCommand]
    private async Task SearchNearbyAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ClearError();
        try
        {
            var location = await _locationService.GetCurrentLocationAsync();
            if (location == null)
            {
                SetError("Could not get your location. Please enable location permissions.");
                return;
            }

            CurrentLocationText = $"Lat: {location.Value.Latitude:F4}, Lon: {location.Value.Longitude:F4}";
            var result = await _rentalService.GetNearbyItemsAsync(location.Value.Latitude, location.Value.Longitude, RadiusKm);
            NearbyItems = new ObservableCollection<Item>(result);

            if (!NearbyItems.Any())
                SetError($"No items found within {RadiusKm}km.");
        }
        catch (Exception ex)
        {
            SetError($"Search failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectItemAsync(Item item)
    {
        await _navigationService.NavigateToAsync("ItemDetailPage", new Dictionary<string, object>
        {
            { "ItemId", item.Id }
        });
    }
}
