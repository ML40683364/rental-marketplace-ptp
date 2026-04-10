using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Database.Models;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class RentalsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Rental> myRentals = new();

    [ObservableProperty]
    private ObservableCollection<Rental> rentalsOnMyItems = new();

    // i added INavigationService here so the Leave Review button can navigate to ReviewsPage
    public RentalsViewModel(IRentalService rentalService, IAuthenticationService authService, INavigationService navigationService)
    {
        _rentalService = rentalService;
        _authService = authService;
        _navigationService = navigationService;
        Title = "My Rentals";
    }

    [RelayCommand]
    private async Task LoadRentalsAsync()
    {
        if (_authService.CurrentUser == null) return;
        IsBusy = true;
        ClearError();
        try
        {
            var mine = await _rentalService.GetMyRentalsAsync(_authService.CurrentUser.Id);
            MyRentals = new ObservableCollection<Rental>(mine);

            var onMyItems = await _rentalService.GetRentalsForMyItemsAsync(_authService.CurrentUser.Id);
            RentalsOnMyItems = new ObservableCollection<Rental>(onMyItems);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rentals: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ApproveRentalAsync(Rental rental)
    {
        try
        {
            await _rentalService.ApproveRentalAsync(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task RejectRentalAsync(Rental rental)
    {
        try
        {
            await _rentalService.RejectRentalAsync(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    // owner taps this after approving - marks item as physically handed over
    [RelayCommand]
    private async Task MarkAsOutForRentAsync(Rental rental)
    {
        try
        {
            await _rentalService.MarkAsOutForRentAsync(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    // borrower taps this when they physically return the item
    [RelayCommand]
    private async Task MarkAsReturnedAsync(Rental rental)
    {
        try
        {
            await _rentalService.MarkAsReturnedAsync(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task CompleteRentalAsync(Rental rental)
    {
        try
        {
            await _rentalService.CompleteRentalAsync(rental.Id);
            await LoadRentalsAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    // i added this so the borrower can navigate to ReviewsPage after a rental is Completed
    // the rentalId is passed so ReviewsViewModel knows which rental to submit the review for
    [RelayCommand]
    private async Task LeaveReviewAsync(Rental rental)
    {
        await _navigationService.NavigateToAsync("ReviewsPage", new Dictionary<string, object>
        {
            { "RentalId", rental.Id }
        });
    }
}
