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

    [ObservableProperty]
    private string itemTitle = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string dailyRateText = string.Empty;

    [ObservableProperty]
    private string location = string.Empty;

    public CreateItemViewModel(IRentalService rentalService, IAuthenticationService authService, INavigationService navigationService)
    {
        _rentalService = rentalService;
        _authService = authService;
        _navigationService = navigationService;
        Title = "List an Item";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_authService.CurrentUser == null) return;

        if (string.IsNullOrWhiteSpace(ItemTitle) || string.IsNullOrWhiteSpace(Location))
        {
            SetError("Title and location are required.");
            return;
        }

        if (!decimal.TryParse(DailyRateText, out var dailyRate) || dailyRate <= 0)
        {
            SetError("Please enter a valid daily rate.");
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var item = new Item
            {
                Title = ItemTitle,
                Description = Description,
                DailyRate = dailyRate,
                Location = Location,
                OwnerId = _authService.CurrentUser.Id,
                IsAvailable = true
            };

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
