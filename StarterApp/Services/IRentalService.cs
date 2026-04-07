using StarterApp.Database.Models;

namespace StarterApp.Services;

public interface IRentalService
{
    Task<List<Item>> GetAvailableItemsAsync();
    Task<List<Item>> GetNearbyItemsAsync(double latitude, double longitude, double radiusKm = 10);
    Task<Item?> GetItemByIdAsync(int id);
    Task<Item> CreateItemAsync(Item item);

    Task<Rental> RequestRentalAsync(int itemId, int renterId, DateTime start, DateTime end);
    Task<Rental> ApproveRentalAsync(int rentalId);
    Task<Rental> RejectRentalAsync(int rentalId);
    Task<Rental> MarkAsOutForRentAsync(int rentalId);
    Task<Rental> CompleteRentalAsync(int rentalId);
    Task<List<Rental>> GetMyRentalsAsync(int renterId);
    Task<List<Rental>> GetRentalsForMyItemsAsync(int ownerId);

    Task<Item> UpdateItemAsync(int itemId, string title, string description, decimal dailyRate, bool isAvailable);
    Task DeleteItemAsync(int itemId);

    Task<Review> SubmitReviewAsync(int rentalId, int reviewerId, int rating, string comment);
    Task<List<Review>> GetReviewsForItemAsync(int itemId);
    Task<double> GetAverageRatingAsync(int itemId);
}
