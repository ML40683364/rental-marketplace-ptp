// RegisterPage.xaml.cs — code-behind for RegisterPage (new user sign-up)

using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from RegisterPage.xaml
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }
}