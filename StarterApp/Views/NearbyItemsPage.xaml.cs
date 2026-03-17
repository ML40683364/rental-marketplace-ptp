// NearbyItemsPage.xaml.cs — code-behind for NearbyItemsPage (GPS-based item search)

using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class NearbyItemsPage : ContentPage
{
    public NearbyItemsPage(NearbyItemsViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from NearbyItemsPage.xaml
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }
}
