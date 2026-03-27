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
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Category selectedCategory;

    // default to Edinburgh — makes sense since that's where the uni is
    // user can override by tapping Get My Location if they want
    private double? _latitude = 55.9533;
    private double? _longitude = -3.1883;

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
                SetError("Could not get location. Please enable location permissions.");
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

    // importent!!! - When the user taps "Save Item" this runs.
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

        IsBusy = true;
        ClearError();
        try
        {

            // after validation passes, it builds the item: 
            // It takes everything the user typed and packages it into an Item object.
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


            //  Takes the user back to the previous screen.
            await _rentalService.CreateItemAsync(item);
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to create item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}
