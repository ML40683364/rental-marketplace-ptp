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

    // i added this to hold the categories for the filter picker
    // it gets filled when the page loads by calling LoadCategoriesAsync
    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    // this holds whichever category the user picks from the dropdown
    // it starts as null which means "show all categories" (no filter applied)
    [ObservableProperty]
    private Category? selectedCategory;

    // i added NotifyPropertyChangedFor so the buttons automatically enable/disable
    // when the page number changes - without this the UI wouldnt know to refresh the buttons
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
    [NotifyPropertyChangedFor(nameof(HasNextPage))]
    [NotifyPropertyChangedFor(nameof(PageInfo))]
    private int currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNextPage))]
    [NotifyPropertyChangedFor(nameof(PageInfo))]
    private int totalPages = 1;

    // these are used in the XAML to enable/disable the Next and Previous buttons
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    // shows "Page 2 of 5" in the middle of the pagination bar
    public string PageInfo => $"Page {CurrentPage} of {TotalPages}";

    public ItemsListViewModel(IRentalService rentalService, INavigationService navigationService)
    {
        _rentalService = rentalService;
        _navigationService = navigationService;
        Title = "Browse Items";
    }

    [RelayCommand]

    // it fetches items from the API, fills the list
    // i updated this to also pass the search text, selected category and page number to the API
    // the API handles the actual filtering - i just send the values as query parameters
    private async Task LoadItemsAsync() => await RunWithLoadingAndErrorHandlingAsync(async () =>
    {
        var categorySlug = SelectedCategory?.Id == 0 ? null : SelectedCategory?.Slug;
        var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
        var result = await _rentalService.GetAvailableItemsAsync(categorySlug, search, CurrentPage);
        Items = new ObservableCollection<Item>(result.Items);
        TotalPages = result.TotalPages == 0 ? 1 : result.TotalPages;
    }, "Failed to load items");

    // i added this to load the category list from the API when the page opens
    // the categories go into a Picker in the XAML so the user can filter by category
    // if this fails i dont want to crash the whole page so i just swallow the error
    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var result = await _rentalService.GetCategoriesAsync();

            // i prepend an "All Categories" option so the user can clear the filter
            // i give it Id=0 so i can check for it in LoadItemsAsync above
            var allOption = new Category { Id = 0, Name = "All Categories", Slug = string.Empty };
            Categories = new ObservableCollection<Category>(result.Prepend(allOption));

            // explicitly set the selected category to "All" so MAUI doesnt auto-pick the first item
            // if i dont do this MAUI sometimes selects the first real category on its own
            SelectedCategory = allOption;
        }
        catch
        {
            // categories not loading shouldnt block the rest of the page from working
        }
    }

    // moves to the next page - the button is disabled when HasNextPage is false
    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!HasNextPage) return;
        CurrentPage++;
        await LoadItemsAsync();
    }

    // moves to the previous page - the button is disabled when HasPreviousPage is false
    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (!HasPreviousPage) return;
        CurrentPage--;
        await LoadItemsAsync();
    }

    // auto-reload when the user picks a different category from the dropdown
    // resets to page 1 so results start from the beginning
    partial void OnSelectedCategoryChanged(Category? value)
    {
        CurrentPage = 1;
        LoadItemsCommand.Execute(null);
    }

    // clears both filters, resets to page 1 and reloads everything
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedCategory = Categories.FirstOrDefault(); // reset to "All Categories"
        CurrentPage = 1;
        await LoadItemsAsync();
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
