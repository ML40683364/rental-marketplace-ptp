using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

// go look in the StarterApp.Database.Models folder - that's where Item and Rental are.
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class ItemsListViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]





    // this is holds the list - UI automatically updates when this changes
    // C# knows Item means Item.cs from that folder because of that import at the top
    private ObservableCollection<Item> items = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    public ItemsListViewModel(IRentalService rentalService, INavigationService navigationService)
    {
        _rentalService = rentalService;
        _navigationService = navigationService;
        Title = "Browse Items";
    }

    [RelayCommand]


    // it fetches items from the API, fills the list
    private async Task LoadItemsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ClearError();
        try
        {
            var result = await _rentalService.GetAvailableItemsAsync();
            Items = new ObservableCollection<Item>(result);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]


    // When tap an item, navigates to ItemDetailPage
    private async Task SelectItemAsync(Item item)
    {
        await _navigationService.NavigateToAsync("ItemDetailPage", new Dictionary<string, object>
        {
            { "ItemId", item.Id }
        });
    }

    [RelayCommand]




    // When tap "+ List Item", navigates to CreateItemPage
    private async Task CreateItemAsync()
    {
        await _navigationService.NavigateToAsync("CreateItemPage");
    }
}
