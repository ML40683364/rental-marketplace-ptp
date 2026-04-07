// ReviewsPage.xaml.cs — code-behind for ReviewsPage (submit a review and see existing reviews)

using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ReviewsPage : ContentPage
{
    public ReviewsPage(ReviewsViewModel viewModel)
    {
        InitializeComponent();          // builds the UI from ReviewsPage.xaml
        BindingContext = viewModel;     // connects the UI to the ViewModel so bindings work
    }
}
