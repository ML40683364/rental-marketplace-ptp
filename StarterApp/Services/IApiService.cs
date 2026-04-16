using StarterApp.Database.Models;

namespace StarterApp.Services;

/// <summary>
/// Holds the JWT token we get back after a successful login.
/// I store this so every API request can attach it in the Authorization header.
/// </summary>
public class AuthToken
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int UserId { get; set; }
}

/// <summary>
/// The data we send to the API when creating a new item listing.
/// I kept this separate from the Item model because the API only needs
/// certain fields - not everything in the database model.
/// </summary>
public class CreateItemRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal DailyRate { get; set; }
    public int CategoryId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

/// <summary>
/// The data we send when updating an existing item.
/// Only includes the fields the owner is allowed to change.
/// </summary>
public class UpdateItemRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Defines every HTTP call our app can make to the shared university API.
/// ApiService implements this with real network calls.
/// Having it as an interface means tests can use a fake version
/// without hitting the actual API.
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Creates a new user account on the API.
    /// </summary>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="email">Must be unique across all users</param>
    /// <param name="password">Must meet the API's password requirements</param>
    Task<User> RegisterAsync(string firstName, string lastName, string email, string password);

    /// <summary>
    /// Logs in and returns a JWT token we use for all authenticated requests.
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <param name="password">The user's password</param>
    /// <returns>An AuthToken containing the JWT string and expiry date</returns>
    Task<AuthToken> LoginAsync(string email, string password);

    /// <summary>
    /// Gets the profile of whoever is currently logged in.
    /// Uses the stored JWT token to identify the user.
    /// </summary>
    Task<User> GetCurrentUserAsync();

    /// <summary>
    /// Gets the public profile of any user by their ID.
    /// Used on the UserDetailPage to show owner information.
    /// </summary>
    /// <param name="userId">The ID of the user to look up</param>
    Task<User> GetUserProfileAsync(int userId);

    /// <summary>
    /// Returns a page of items, optionally filtered by category or search text.
    /// Also returns the total page count so the UI can show pagination buttons.
    /// </summary>
    /// <param name="category">Optional category slug e.g. "tools"</param>
    /// <param name="search">Optional text to search in title and description</param>
    /// <param name="page">Page number starting from 1</param>
    Task<(List<Item> Items, int TotalPages)> GetItemsAsync(string? category = null, string? search = null, int page = 1);

    /// <summary>
    /// Finds items near a location using the API's PostGIS spatial search.
    /// Much faster than fetching all items and filtering on the client.
    /// </summary>
    /// <param name="lat">User's latitude</param>
    /// <param name="lon">User's longitude</param>
    /// <param name="radius">Search radius in kilometres, defaults to 5km</param>
    /// <param name="category">Optional category filter</param>
    Task<List<Item>> GetNearbyItemsAsync(double lat, double lon, double radius = 5.0, string category = null);

    /// <summary>
    /// Gets a single item by its ID.
    /// </summary>
    /// <param name="id">The item ID</param>
    Task<Item> GetItemAsync(int id);

    /// <summary>
    /// Creates a new item listing on the API.
    /// </summary>
    /// <param name="request">The item details to create</param>
    Task<Item> CreateItemAsync(CreateItemRequest request);

    /// <summary>
    /// Updates an existing item. Only the owner can do this.
    /// </summary>
    /// <param name="id">The item ID to update</param>
    /// <param name="request">The updated fields</param>
    Task<Item> UpdateItemAsync(int id, UpdateItemRequest request);

    /// <summary>
    /// Soft deletes an item by marking it unavailable.
    /// The API has no real DELETE endpoint so this is the workaround.
    /// </summary>
    /// <param name="id">The item ID to delete</param>
    Task DeleteItemAsync(int id);

    /// <summary>
    /// Gets all available categories e.g. Tools, Camping, Sports.
    /// Used to populate the filter dropdown on the items list page.
    /// </summary>
    Task<List<Category>> GetCategoriesAsync();

    /// <summary>
    /// Sends a rental request to the API for a specific item and date range.
    /// </summary>
    /// <param name="itemId">The item being requested</param>
    /// <param name="startDate">When the rental starts</param>
    /// <param name="endDate">When the rental ends</param>
    Task<Rental> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets rentals for items the current user owns.
    /// These are requests coming in that they need to approve or reject.
    /// </summary>
    /// <param name="status">Optional filter e.g. "Requested" to see only pending ones</param>
    Task<List<Rental>> GetIncomingRentalsAsync(string status = null);

    /// <summary>
    /// Gets rentals the current user has requested from other people.
    /// </summary>
    /// <param name="status">Optional filter by status</param>
    Task<List<Rental>> GetOutgoingRentalsAsync(string status = null);

    /// <summary>
    /// Gets a single rental by its ID including full status history.
    /// </summary>
    /// <param name="id">The rental ID</param>
    Task<Rental> GetRentalAsync(int id);

    /// <summary>
    /// Updates the status of a rental e.g. from Requested to Approved.
    /// The API validates that the transition is allowed before accepting it.
    /// </summary>
    /// <param name="rentalId">The rental to update</param>
    /// <param name="status">The new status string e.g. "Approved"</param>
    Task UpdateRentalStatusAsync(int rentalId, string status);

    /// <summary>
    /// Submits a review for a completed rental.
    /// </summary>
    /// <param name="rentalId">The rental being reviewed, must be Completed</param>
    /// <param name="rating">Star rating from 1 to 5</param>
    /// <param name="comment">Optional written feedback</param>
    Task<Review> CreateReviewAsync(int rentalId, int rating, string comment);

    /// <summary>
    /// Gets all reviews for a specific item, paginated.
    /// </summary>
    /// <param name="itemId">The item to get reviews for</param>
    /// <param name="page">Page number starting from 1</param>
    Task<List<Review>> GetItemReviewsAsync(int itemId, int page = 1);

    /// <summary>
    /// Gets all reviews left for a specific user as an owner.
    /// Used on the user profile page to show their reputation.
    /// </summary>
    /// <param name="userId">The user to get reviews for</param>
    /// <param name="page">Page number starting from 1</param>
    Task<List<Review>> GetUserReviewsAsync(int userId, int page = 1);
}
