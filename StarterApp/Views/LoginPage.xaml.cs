// LoginPage.xaml.cs — code-behind for LoginPage

using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from LoginPage.xaml
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }

    // Previously this method pre-filled the form with admin@company.com and Admin123! as a dev shortcut.
    // Removed hardcoded test credentials because leaving them in would mean any user opening the app
    // would see someone else's email and password pre-filled, which is a security risk and looks unprofessional.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        EmailEntry.Focus();
    }
}