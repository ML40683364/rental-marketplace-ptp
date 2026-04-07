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

    // runs every time this page appears on screen
    // NOTE: pre-fills credentials for development/testing — remove before releasing the app
    protected override void OnAppearing()
    {
        base.OnAppearing();
        EmailEntry.Focus();                         // puts the cursor in the email field
        EmailEntry.Text = "admin@company.com";      // dev shortcut — pre-filled test email
        PasswordEntry.Text = "Admin123!";           // dev shortcut — pre-filled test password
    }
}