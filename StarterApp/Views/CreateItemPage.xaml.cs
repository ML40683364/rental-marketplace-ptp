// CreateItemPage.xaml.cs — code-behind for CreateItemPage
// The ViewModel is injected (passed in from outside) rather than created here directly.
// This is called dependency injection — the app creates the ViewModel and hands it to this page.

using StarterApp.ViewModels;

// namespace tells the compiler this class belongs to the Views folder of StarterApp
namespace StarterApp.Views;

// partial = this class is split across CreateItemPage.xaml.cs (this file) and CreateItemPage.xaml (the UI)
// : ContentPage means this class inherits everything needed to be a page in the app
public partial class CreateItemPage : ContentPage
{
    // constructor receives a ready-made CreateItemViewModel from the app (dependency injection)
    // contrast with AboutPage where the ViewModel is created directly in the XAML — this pattern
    // is more flexible because the ViewModel can be pre-loaded with data before the page opens
    public CreateItemPage(CreateItemViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from CreateItemPage.xaml
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }
}
