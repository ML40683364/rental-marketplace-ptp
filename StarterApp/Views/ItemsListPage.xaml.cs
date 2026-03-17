// ItemsListPage.xaml.cs — code-behind for ItemsListPage
// Stores the ViewModel in a field so OnAppearing can use it to trigger a data refresh

using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ItemsListPage : ContentPage
{
    // stored so we can call it in OnAppearing below
    private readonly ItemsListViewModel _viewModel;

    public ItemsListPage(ItemsListViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from ItemsListPage.xaml
        _viewModel = viewModel;         // save the ViewModel for use in OnAppearing
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }

    // OnAppearing runs every time the user navigates TO this page (not just the first time)
    // this ensures the list is always fresh, e.g. after adding a new item
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadItemsCommand.Execute(null); // tells the ViewModel to fetch the items
    }
}
