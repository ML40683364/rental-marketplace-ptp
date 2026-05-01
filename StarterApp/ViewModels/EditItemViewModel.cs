using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Services;

namespace StarterApp.ViewModels;

// EditItemViewModel pre-fills the form with the item's current values.
// When the owner taps "Save", it calls UpdateItemAsync on the service.
// Owner-only: ItemDetailViewModel only navigates here if the current user owns the item.
[QueryProperty(nameof(ItemId), "ItemId")]
[QueryProperty(nameof(ItemTitle), "ItemTitle")]
[QueryProperty(nameof(ItemDescription), "ItemDescription")]
[QueryProperty(nameof(DailyRateText), "DailyRateText")]
[QueryProperty(nameof(IsAvailable), "IsAvailable")]
public partial class EditItemViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private string itemTitle = string.Empty;

    [ObservableProperty]
    private string itemDescription = string.Empty;

    // stored as text so the Entry field binds to a string, validated on save
    [ObservableProperty]
    private string dailyRateText = string.Empty;

    [ObservableProperty]
    private bool isAvailable;

    public EditItemViewModel(IRentalService rentalService, INavigationService navigationService)
    {
        _rentalService = rentalService;
        _navigationService = navigationService;
        Title = "Edit Item";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
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

        await RunWithLoadingAndErrorHandlingAsync(async () =>
        {
            await _rentalService.UpdateItemAsync(ItemId, ItemTitle, ItemDescription, dailyRate, IsAvailable);
            await _navigationService.NavigateBackAsync();
        }, "Failed to update item");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        // My first attempt was to call DELETE /items/{id} on the API, which Claude suggested,
        // but every call came back with a 404 error. After checking the Swagger docs I realised
        // the API has no DELETE endpoint at all - removing a record would orphan every rental and
        // review that references it, leaving broken foreign keys in the database. The solution was
        // to use PUT /items/{id} with isAvailable = false, which hides the item from Browse Items
        // without touching the underlying record or any of its linked data.

        bool confirmed = await Shell.Current.DisplayAlert(
            "Remove Item",
            "Are you sure you want to remove this item? It will no longer appear in Browse Items.",
            "Remove",
            "Cancel");

        if (!confirmed) return;

        await RunWithLoadingAndErrorHandlingAsync(async () =>
        {
            // PUT /items/{id} with isAvailable = false is the only way to hide an item -
            // the API has no DELETE endpoint so this is a soft delete.
            // Go back twice: EditItemPage → ItemDetailPage → ItemsListPage.
            decimal.TryParse(DailyRateText, out var rate);
            await _rentalService.UpdateItemAsync(ItemId, ItemTitle, ItemDescription,
                rate, false);
            await _navigationService.NavigateBackAsync();
            await _navigationService.NavigateBackAsync();
        }, "Failed to remove item");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}
