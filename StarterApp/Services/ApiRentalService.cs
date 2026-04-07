using StarterApp.Database.Models;

namespace StarterApp.Services;

// This is the API version of IRentalService - mirrors what RentalService does
// but talks to the shared API instead of the local database.
// Same idea as ApiAuthenticationService vs LocalAuthenticationService.
// MauiProgram.cs decides which one gets injected based on the useSharedApi flag.
public class ApiRentalService : IRentalService
{
    private readonly IApiService _apiService;

    public ApiRentalService(IApiService apiService)
    {
        _apiService = apiService;
    }

    // --- Items ---

    public Task<List<Item>> GetAvailableItemsAsync()
        => _apiService.GetItemsAsync();

    public Task<List<Item>> GetNearbyItemsAsync(double latitude, double longitude, double radiusKm = 10)
        => _apiService.GetNearbyItemsAsync(latitude, longitude, radiusKm);

    public Task<Item?> GetItemByIdAsync(int id)
        => _apiService.GetItemAsync(id)!;

    // the local Item model has more fields than the API needs, so we map only what's required
    public async Task<Item> CreateItemAsync(Item item)
    {
        var request = new CreateItemRequest
        {
            Title = item.Title,
            Description = item.Description,
            DailyRate = item.DailyRate,
            CategoryId = item.CategoryId ?? 1,
            Latitude = item.Latitude ?? 0,
            Longitude = item.Longitude ?? 0
        };
        return await _apiService.CreateItemAsync(request);
    }

    public async Task<Item> UpdateItemAsync(int itemId, string title, string description, decimal dailyRate, bool isAvailable)
    {
        var request = new UpdateItemRequest
        {
            Title = title,
            Description = description,
            DailyRate = dailyRate,
            IsAvailable = isAvailable
        };
        return await _apiService.UpdateItemAsync(itemId, request);
    }

    public Task DeleteItemAsync(int itemId)
        => _apiService.DeleteItemAsync(itemId);

    // --- Rentals ---

    // the renterId param exists in IRentalService for the local DB version
    // the API doesn't need it - it figures out who the borrower is from the JWT token
    public Task<Rental> RequestRentalAsync(int itemId, int renterId, DateTime start, DateTime end)
        => _apiService.RequestRentalAsync(itemId, start, end);

    // status update then fetch the updated rental to return it
    public async Task<Rental> ApproveRentalAsync(int rentalId)
    {
        await _apiService.UpdateRentalStatusAsync(rentalId, "Approved");
        return await _apiService.GetRentalAsync(rentalId);
    }

    public async Task<Rental> RejectRentalAsync(int rentalId)
    {
        await _apiService.UpdateRentalStatusAsync(rentalId, "Rejected");
        return await _apiService.GetRentalAsync(rentalId);
    }

    public async Task<Rental> MarkAsOutForRentAsync(int rentalId)
    {
        await _apiService.UpdateRentalStatusAsync(rentalId, "Out for Rent");
        return await _apiService.GetRentalAsync(rentalId);
    }

    public async Task<Rental> CompleteRentalAsync(int rentalId)
    {
        await _apiService.UpdateRentalStatusAsync(rentalId, "Completed");
        return await _apiService.GetRentalAsync(rentalId);
    }

    // same as renterId above - ownerId isn't needed, API uses the token
    public Task<List<Rental>> GetMyRentalsAsync(int renterId)
        => _apiService.GetOutgoingRentalsAsync();

    public Task<List<Rental>> GetRentalsForMyItemsAsync(int ownerId)
        => _apiService.GetIncomingRentalsAsync();

    // --- Reviews ---

    // reviewerId ignored for same reason - inferred from the token on the API side
    public Task<Review> SubmitReviewAsync(int rentalId, int reviewerId, int rating, string comment)
        => _apiService.CreateReviewAsync(rentalId, rating, comment);

    public async Task<List<Review>> GetReviewsForItemAsync(int itemId)
        => await _apiService.GetItemReviewsAsync(itemId);

    // the API doesn't have a dedicated average rating endpoint so we calculate it ourselves
    public async Task<double> GetAverageRatingAsync(int itemId)
    {
        var reviews = await _apiService.GetItemReviewsAsync(itemId);
        if (!reviews.Any()) return 0;
        return reviews.Average(r => r.Rating);
    }
}
