using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class CreateItemViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    // used by CreateItemViewModel and NearbyItemsViewModel to get the GPS coordinates from the phone
    private readonly ILocationService _locationService;
    private readonly IApiService _apiService;

    [ObservableProperty]

    //  Each string matches a field in CreateItemPage.xaml. When the user types in the title field, it automatically updates itemTitle, description, dailyRateText, locationText,  ObservableCollection<Category> categories, Category selectedCategory  
    private string itemTitle = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string dailyRateText = string.Empty;

    // shows the user their detected coordinates on screen
    [ObservableProperty]
    private string locationText = "Default: Edinburgh (55.9533, -3.1883)";

    [ObservableProperty]
    private Color locationTextColor = Color.FromArgb("#555555");

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Category selectedCategory;

    // default to Edinburgh using the shared constant from LocationService
    private double? _latitude = LocationService.DefaultLatitude;
    private double? _longitude = LocationService.DefaultLongitude;

    public CreateItemViewModel(IRentalService rentalService, IAuthenticationService authService,
        INavigationService navigationService, ILocationService locationService, IApiService apiService)
    {
        _rentalService = rentalService;
        _authService = authService;
        _navigationService = navigationService;
        _locationService = locationService;
        _apiService = apiService;
        Title = "List an Item";
    }

    // loads categories from the API when the page appears
    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var result = await _apiService.GetCategoriesAsync();
            Categories = new ObservableCollection<Category>(result);
            if (Categories.Any())
                SelectedCategory = Categories.First();
        }
        catch (Exception ex)
        {
            SetError($"Could not load categories: {ex.Message}");
        }
    }

    // gets the device GPS coordinates and shows them to the user
    [RelayCommand]
    private async Task GetLocationAsync()
    {
        try
        {
            var location = await _locationService.GetCurrentLocationAsync();
            if (location == null)
            {
                _latitude = LocationService.DefaultLatitude;
                _longitude = LocationService.DefaultLongitude;
                LocationText = "⚠ GPS unavailable  defaulting to Edinburgh (55.9533, -3.1883)";
                LocationTextColor = Color.FromArgb("#FF9800");
                ClearError();
                return;
            }
            _latitude = location.Value.Latitude;
            _longitude = location.Value.Longitude;
            LocationText = $"Location set: {_latitude:F4}, {_longitude:F4}";
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Location error: {ex.Message}");
        }
    }

    [RelayCommand]

    // When the user taps "Save Item" this runs.
    // checks - is the user logged in? Is the title empty? Is the daily rate a valid number? Has the user got a location? Has the user selected a category?.....
    private async Task SaveAsync()
    {
        if (_authService.CurrentUser == null) return;

        if (string.IsNullOrWhiteSpace(ItemTitle))
        {
            SetError("Title is required.");
            return;
        }

        if (!decimal.TryParse(DailyRateText, out var dailyRate) || dailyRate <= 0)
        {
            SetError("Please enter a valid daily rate.");
            return;
        }

        if (_latitude == null || _longitude == null)
        {
            SetError("Please get your location first.");
            return;
        }

        if (SelectedCategory == null)
        {
            SetError("Please select a category.");
            return;
        }

        await RunWithLoadingAndErrorHandlingAsync(async () =>
        {
            var item = new Item
            {
                Title = ItemTitle,
                Description = Description,
                DailyRate = dailyRate,
                CategoryId = SelectedCategory.Id,
                Latitude = _latitude,
                Longitude = _longitude,
                OwnerId = _authService.CurrentUser.Id,
                IsAvailable = true
            };
            await _rentalService.CreateItemAsync(item);
            await _navigationService.NavigateBackAsync();
        }, "Failed to create item");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}
