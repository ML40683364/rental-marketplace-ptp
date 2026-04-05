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

    // true when the logged-in user is the owner of this item — controls Edit button visibility
    [ObservableProperty]
    private bool isOwner;

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
                IsOwner = _authService.CurrentUser?.Id == Item.OwnerId;
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
    private async Task EditItemAsync()
    {
        if (Item == null) return;

        if (!IsOwner)
        {
            SetError("You can only edit your own items.");
            return;
        }
        // pass all current values to EditItemPage so the form is pre-filled
        await _navigationService.NavigateToAsync("EditItemPage", new Dictionary<string, object>
        {
            { "ItemId", Item.Id },
            { "ItemTitle", Item.Title ?? string.Empty },
            { "ItemDescription", Item.Description ?? string.Empty },
            { "DailyRateText", Item.DailyRate.ToString("F2") },
            { "IsAvailable", Item.IsAvailable }
        });
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
