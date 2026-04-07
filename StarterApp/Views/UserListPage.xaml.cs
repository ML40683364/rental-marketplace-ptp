// UserListPage.xaml.cs — code-behind for UserListPage (admin-only: search and manage all users)

using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class UserListPage : ContentPage
{
    public UserListPage(UserListViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from UserListPage.xaml
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }
}