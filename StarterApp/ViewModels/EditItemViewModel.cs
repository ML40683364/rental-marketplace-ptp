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

        IsBusy = true;
        ClearError();
        try
        {
            await _rentalService.UpdateItemAsync(ItemId, ItemTitle, ItemDescription, dailyRate, IsAvailable);
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to update item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        // ask the user to confirm before deleting — this cannot be undone
        bool confirmed = await Shell.Current.DisplayAlert(
            "Delete Item",
            "Are you sure you want to delete this item? This cannot be undone.",
            "Delete",
            "Cancel");

        if (!confirmed) return;

        IsBusy = true;
        ClearError();
        try
        {
            await _rentalService.DeleteItemAsync(ItemId);
            // go all the way back to the items list after deletion
            await Shell.Current.GoToAsync("//ItemsListPage");
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete item: {ex.Message}");
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
