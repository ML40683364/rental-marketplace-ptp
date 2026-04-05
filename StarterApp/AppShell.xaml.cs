using StarterApp.ViewModels;
using StarterApp.Views;

namespace StarterApp;

public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("ItemsListPage", typeof(ItemsListPage));
        Routing.RegisterRoute("ItemDetailPage", typeof(ItemDetailPage));
        Routing.RegisterRoute("CreateItemPage", typeof(CreateItemPage));
        Routing.RegisterRoute("NearbyItemsPage", typeof(NearbyItemsPage));
        Routing.RegisterRoute("RentalsPage", typeof(RentalsPage));
        Routing.RegisterRoute("ReviewsPage", typeof(ReviewsPage));
        Routing.RegisterRoute("EditItemPage", typeof(EditItemPage));
    }
}
