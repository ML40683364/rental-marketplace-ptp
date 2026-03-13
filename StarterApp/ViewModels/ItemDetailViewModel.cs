using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(ItemId), "ItemId")]
public partial class ItemDetailViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private Item? item;

    [ObservableProperty]
    private ObservableCollection<Review> reviews = new();

    [ObservableProperty]
    private double averageRating;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime endDate = DateTime.Today.AddDays(1);

    public ItemDetailViewModel(IRentalService rentalService, IAuthenticationService authService, INavigationService navigationService)
    {
        _rentalService = rentalService;
        _authService = authService;
        _navigationService = navigationService;
        Title = "Item Details";
    }

    partial void OnItemIdChanged(int value) => _ = LoadItemAsync();

    [RelayCommand]
    private async Task LoadItemAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ClearError();
        try
        {
            Item = await _rentalService.GetItemByIdAsync(ItemId);
            if (Item != null)
            {
                Title = Item.Title;
                var reviewList = await _rentalService.GetReviewsForItemAsync(ItemId);
                Reviews = new ObservableCollection<Review>(reviewList);
                AverageRating = await _rentalService.GetAverageRatingAsync(ItemId);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RequestRentalAsync()
    {
        if (_authService.CurrentUser == null) return;
        IsBusy = true;
        ClearError();
        try
        {
            await _rentalService.RequestRentalAsync(ItemId, _authService.CurrentUser.Id, StartDate, EndDate);
            await _navigationService.NavigateToAsync("RentalsPage");
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
