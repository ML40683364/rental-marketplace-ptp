// ItemDetailPage.xaml.cs — code-behind for ItemDetailPage
// ViewModel is injected (passed in) because it needs to be pre-loaded with the specific item to display

using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ItemDetailPage : ContentPage
{
    public ItemDetailPage(ItemDetailViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from ItemDetailPage.xaml
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }
}
