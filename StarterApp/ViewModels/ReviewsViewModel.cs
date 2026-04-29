using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Services;

namespace StarterApp.ViewModels;

[QueryProperty(nameof(RentalId), "RentalId")]
public partial class ReviewsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int rentalId;

    // slider returns a double but the API only accepts integer 1-5
    // i use (int) cast when submitting to make sure it sends a whole number
    // NotifyPropertyChangedFor means StarDisplay updates live as the slider moves
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StarDisplay))]
    private double selectedRating = 5;

    // builds a visual star string like ★★★☆☆ for 3/5
    // filled stars = selected rating, empty stars = the rest up to 5
    public string StarDisplay
    {
        get
        {
            var filled = (int)SelectedRating;
            return new string('★', filled) + new string('☆', 5 - filled);
        }
    }

    [ObservableProperty]
    private string comment = string.Empty;

    public ReviewsViewModel(IRentalService rentalService, IAuthenticationService authService, INavigationService navigationService)
    {
        _rentalService = rentalService;
        _authService = authService;
        _navigationService = navigationService;
        Title = "Leave a Review";
    }

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (_authService.CurrentUser == null) return;
        await RunWithLoadingAndErrorHandlingAsync(async () =>
        {
            // cast to int because the API expects integer rating 1-5, slider gives a double
            await _rentalService.SubmitReviewAsync(RentalId, _authService.CurrentUser.Id, (int)SelectedRating, Comment);
            Comment = string.Empty;
            SelectedRating = 5;
            await _navigationService.NavigateBackAsync();
        });
    }
}
