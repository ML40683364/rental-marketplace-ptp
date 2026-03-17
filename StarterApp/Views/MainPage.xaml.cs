// MainPage.xaml.cs — code-behind for MainPage (the home dashboard after login)

using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from MainPage.xaml
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }
}