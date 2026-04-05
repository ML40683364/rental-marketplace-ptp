using StarterApp.Database.Models;

namespace StarterApp.Services;

// These are the data shapes we send TO the API.
// Kept here because they're only used for API communication,
// not for the local database.

public class AuthToken
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int UserId { get; set; }
}

// what we send when listing a new item
public class CreateItemRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal DailyRate { get; set; }
    public int CategoryId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

// only the fields the owner is allowed to change
public class UpdateItemRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsAvailable { get; set; }
}

// This interface defines everything our app can do with the shared API.
// ApiService implements this for real HTTP calls.
// A mock version can implement it for testing without hitting the network.
public interface IApiService
{
    // auth - login, register, get profile
    Task<User> RegisterAsync(string firstName, string lastName, string email, string password);
    Task<AuthToken> LoginAsync(string email, string password);
    Task<User> GetCurrentUserAsync();
    Task<User> GetUserProfileAsync(int userId);

    // browsing and managing items
    Task<List<Item>> GetItemsAsync(string category = null, string search = null, int page = 1);
    Task<List<Item>> GetNearbyItemsAsync(double lat, double lon, double radius = 5.0, string category = null);
    Task<Item> GetItemAsync(int id);
    Task<Item> CreateItemAsync(CreateItemRequest request);
    Task<Item> UpdateItemAsync(int id, UpdateItemRequest request);
    Task DeleteItemAsync(int id);

    // categories are used for filtering items
    Task<List<Category>> GetCategoriesAsync();

    // rental workflow - request, approve, reject, complete
    Task<Rental> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate);
    Task<List<Rental>> GetIncomingRentalsAsync(string status = null);
    Task<List<Rental>> GetOutgoingRentalsAsync(string status = null);
    Task<Rental> GetRentalAsync(int id);
    Task UpdateRentalStatusAsync(int rentalId, string status);

    // reviews - submit and read feedback
    Task<Review> CreateReviewAsync(int rentalId, int rating, string comment);
    Task<List<Review>> GetItemReviewsAsync(int itemId, int page = 1);
    Task<List<Review>> GetUserReviewsAsync(int userId, int page = 1);
}
