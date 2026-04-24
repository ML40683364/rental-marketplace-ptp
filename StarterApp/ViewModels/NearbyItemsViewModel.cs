using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel; // NuGet package
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;

    // LocationService is beeing called from the NearbyItemsViewModel and CreateItemViewModel
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
            // gets the GPS location from the phone
            var location = await _locationService.GetCurrentLocationAsync();

            // emulator has no real GPS - fall back to Edinburgh centre so the feature works during testing
            if (location == null)
                location = (55.9533, -3.1883);

            // 55.9533, -3.1883 = St Andrew Square, Edinburgh city centre — a public landmark chosen
            // deliberately so no residential address is ever hardcoded into the app
            CurrentLocationText = location == (55.9533, -3.1883)
                ? "Using default location: Edinburgh"
                : $"Lat: {location.Value.Latitude:F4}, Lon: {location.Value.Longitude:F4}";
            //  passes those coordinates to RentalService to find nearby items
            // IRentalService is being called 
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
        // uses NavigationService to go to the item detail page when user taps an item
        await _navigationService.NavigateToAsync("ItemDetailPage", new Dictionary<string, object>
        {
            { "ItemId", item.Id }
        });
    }
}
