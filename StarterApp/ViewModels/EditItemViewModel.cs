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
        // I originally tried to call DELETE /items/{id} on the API but kept getting 404.
        // After checking the Swagger docs carefully I realised the API has no DELETE endpoint at all.
        // The only way to "remove" an item is to set IsAvailable = false using PUT /items/{id}.
        // This is actually called a "soft delete" — the item stays in the database but
        // disappears from Browse Items because the API only returns IsAvailable = true items.
        //
        // I thought this was strange at first but then I learned this is how most real apps work.
        // Think about Facebook — when you delete a post it does not actually disappear from their
        // servers. Or when you cancel an Amazon order — the record stays for accounting and disputes.
        // Nothing truly disappears from the internet. Data is just hidden, not destroyed.
        //
        // For this app it also makes sense because if we hard-deleted an item, all the rentals
        // and reviews linked to it would become orphaned records pointing to nothing.
        // Soft delete avoids that problem entirely.

        bool confirmed = await Shell.Current.DisplayAlert(
            "Remove Item",
            "Are you sure you want to remove this item? It will no longer appear in Browse Items.",
            "Remove",
            "Cancel");

        if (!confirmed) return;

        IsBusy = true;
        ClearError();
        try
        {
            // I reuse UpdateItemAsync here instead of DeleteItemAsync because the API
            // has no DELETE endpoint — PUT /items/{id} with isAvailable = false is
            // the only way to hide an item from the marketplace.
            // The item disappears from Browse Items immediately after this call
            // because GET /items only returns items where isAvailable = true.
            await _rentalService.UpdateItemAsync(ItemId, ItemTitle, ItemDescription,
                decimal.Parse(DailyRateText), false);

            // I tried GoToAsync("//ItemsListPage") but got an absolute routing error.
            // PopToRootAsync() went too far and logged the user out.
            // The correct fix is to go back twice — first from EditItemPage to ItemDetailPage,
            // then from ItemDetailPage back to ItemsListPage.
            await _navigationService.NavigateBackAsync();
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to remove item: {ex.Message}");
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
