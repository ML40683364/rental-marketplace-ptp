// RentalsPage.xaml.cs — code-behind for RentalsPage (manage your rentals and incoming requests)
// Stores the ViewModel in a field so OnAppearing can trigger a data refresh

using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class RentalsPage : ContentPage
{
    // stored so we can call it in OnAppearing below
    private readonly RentalsViewModel _viewModel;

    public RentalsPage(RentalsViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from RentalsPage.xaml
        _viewModel = viewModel;         // save the ViewModel for use in OnAppearing
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }

    // runs every time the user navigates TO this page
    // ensures rental data is always up to date (e.g. after approving or rejecting a request)
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadRentalsCommand.Execute(null); // tells the ViewModel to fetch rentals
    }
}
