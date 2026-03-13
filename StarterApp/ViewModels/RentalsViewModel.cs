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

    [ObservableProperty]
    private ObservableCollection<Rental> myRentals = new();

    [ObservableProperty]
    private ObservableCollection<Rental> rentalsOnMyItems = new();

    public RentalsViewModel(IRentalService rentalService, IAuthenticationService authService)
    {
        _rentalService = rentalService;
        _authService = authService;
        Title = "My Rentals";
    }

    [RelayCommand]
    private async Task LoadRentalsAsync()
    {
        if (_authService.CurrentUser == null || IsBusy) return;
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
}
