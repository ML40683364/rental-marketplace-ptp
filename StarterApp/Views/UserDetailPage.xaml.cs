// UserDetailPage.xaml.cs — code-behind for UserDetailPage (edit a user's details and roles)
// ViewModel is injected because it needs to be pre-loaded with the specific user to edit

using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class UserDetailPage : ContentPage
{
    public UserDetailPage(UserDetailViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from UserDetailPage.xaml
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }
}