// RegisterPage.xaml.cs — code-behind for RegisterPage (new user sign-up)
// RegisterViewModel is a Singleton so its fields persist between visits.
// OnAppearing clears all fields each time the page appears so a second registration
// does not show the previous user's details.

using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from RegisterPage.xaml
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RegisterViewModel vm)
            vm.ResetForm();
    }
}