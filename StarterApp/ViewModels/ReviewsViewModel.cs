using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
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

    [ObservableProperty]
    private ObservableCollection<Review> reviews = new();

    [ObservableProperty]
    private int selectedRating = 5;

    [ObservableProperty]
    private string comment = string.Empty;

    public ReviewsViewModel(IRentalService rentalService, IAuthenticationService authService, INavigationService navigationService)
    {
        _rentalService = rentalService;
        _authService = authService;
        _navigationService = navigationService;
        Title = "Reviews";
    }

    partial void OnRentalIdChanged(int value) => _ = LoadReviewsAsync();

    [RelayCommand]
    private async Task LoadReviewsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ClearError();
        try
        {
            var rental = await _rentalService.GetReviewsForItemAsync(RentalId);
            Reviews = new ObservableCollection<Review>(rental);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load reviews: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (_authService.CurrentUser == null) return;
        IsBusy = true;
        ClearError();
        try
        {
            await _rentalService.SubmitReviewAsync(RentalId, _authService.CurrentUser.Id, SelectedRating, Comment);
            Comment = string.Empty;
            SelectedRating = 5;
            await LoadReviewsAsync();
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
