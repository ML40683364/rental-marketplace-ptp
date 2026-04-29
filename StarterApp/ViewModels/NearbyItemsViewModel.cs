using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel; // NuGet package
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;

    // LocationService is used by NearbyItemsViewModel and CreateItemViewModel
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
    private async Task SearchNearbyAsync() => await RunWithLoadingAndErrorHandlingAsync(async () =>
    {
        var location = await _locationService.GetCurrentLocationAsync();

        // emulator has no real GPS - fall back to Edinburgh centre so the feature works during testing
        if (location == null)
            location = (LocationService.DefaultLatitude, LocationService.DefaultLongitude);

        CurrentLocationText = location == (LocationService.DefaultLatitude, LocationService.DefaultLongitude)
            ? "Using default location: Edinburgh"
            : $"Lat: {location.Value.Latitude:F4}, Lon: {location.Value.Longitude:F4}";

        var result = await _rentalService.GetNearbyItemsAsync(location.Value.Latitude, location.Value.Longitude, RadiusKm);
        NearbyItems = new ObservableCollection<Item>(result);

        if (!NearbyItems.Any())
            SetError($"No items found within {RadiusKm}km.");
    }, "Search failed");

    [RelayCommand]
    private async Task SelectItemAsync(Item item)
    {
        // uses NavigationService to go to the item detail page when user taps an item
        await _navigationService.NavigateToAsync("ItemDetailPage", new Dictionary<string, object>
        {
            { "ItemId", item.Id }
        });
    }
}
