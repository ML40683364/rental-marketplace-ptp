using StarterApp.Database.Models;

namespace StarterApp.Services;

/// <summary>
/// I created this class because the ViewModel needs more than just a list of items -
/// it also needs to know how many pages exist to enable/disable the Next and Previous buttons.
/// Returning both in one object felt cleaner than making two separate calls.
/// </summary>
public class PagedItemsResult
{
    public List<Item> Items { get; set; } = new();
    public int TotalPages { get; set; } = 1;
    public int CurrentPage { get; set; } = 1;
}

/// <summary>
/// The main service interface for the rental marketplace.
/// All business logic goes through here - items, rentals, and reviews.
/// I learned that keeping this as an interface means I can swap between
/// the local database version and the API version without changing the ViewModels.
/// </summary>
public interface IRentalService
{
    /// <summary>
    /// Returns a page of available items, optionally filtered by category or search text.
    /// I used pagination here because loading all items at once was too slow.
    /// </summary>
    /// <param name="category">Optional category slug to filter by e.g. "tools"</param>
    /// <param name="search">Optional search text to match against item title and description</param>
    /// <param name="page">Page number starting from 1, defaults to 1</param>
    /// <returns>A paged result containing the items and total page count</returns>
    Task<PagedItemsResult> GetAvailableItemsAsync(string? category = null, string? search = null, int page = 1);

    /// <summary>
    /// Returns all categories e.g. Tools, Camping, Sports.
    /// Used to populate the category filter dropdown on the items list page.
    /// </summary>
    Task<List<Category>> GetCategoriesAsync();

    /// <summary>
    /// Finds items within a given radius of a location using GPS coordinates.
    /// This was the trickiest feature to implement - it uses the Haversine formula
    /// to calculate real-world distance between two points on the earth.
    /// </summary>
    /// <param name="latitude">User's current latitude from GPS</param>
    /// <param name="longitude">User's current longitude from GPS</param>
    /// <param name="radiusKm">Search radius in kilometres, defaults to 10km</param>
    Task<List<Item>> GetNearbyItemsAsync(double latitude, double longitude, double radiusKm = 10);

    /// <summary>
    /// Gets a single item by its ID. Returns null if the item doesn't exist.
    /// </summary>
    /// <param name="id">The item ID</param>
    Task<Item?> GetItemByIdAsync(int id);

    /// <summary>
    /// Creates a new item listing in the marketplace.
    /// </summary>
    /// <param name="item">The item to create including title, description, daily rate and location</param>
    Task<Item> CreateItemAsync(Item item);

    /// <summary>
    /// Submits a rental request for an item. I added date overlap checking here
    /// so two people can't book the same item for the same dates.
    /// </summary>
    /// <param name="itemId">The item being requested</param>
    /// <param name="renterId">The user making the request</param>
    /// <param name="start">Rental start date</param>
    /// <param name="end">Rental end date</param>
    Task<Rental> RequestRentalAsync(int itemId, int renterId, DateTime start, DateTime end);

    /// <summary>
    /// Owner approves a rental request, moving it from Requested to Approved.
    /// </summary>
    /// <param name="rentalId">The rental to approve</param>
    Task<Rental> ApproveRentalAsync(int rentalId);

    /// <summary>
    /// Owner rejects a rental request, moving it from Requested to Cancelled.
    /// </summary>
    /// <param name="rentalId">The rental to reject</param>
    Task<Rental> RejectRentalAsync(int rentalId);

    /// <summary>
    /// Marks a rental as Out for Rent when the borrower picks up the item.
    /// Moves status from Approved to OutForRent.
    /// </summary>
    /// <param name="rentalId">The rental to update</param>
    Task<Rental> MarkAsOutForRentAsync(int rentalId);

    /// <summary>
    /// Borrower marks the item as returned. Moves status from OutForRent to Returned.
    /// After this the owner still needs to confirm before it becomes Completed.
    /// </summary>
    /// <param name="rentalId">The rental to update</param>
    Task<Rental> MarkAsReturnedAsync(int rentalId);

    /// <summary>
    /// Owner confirms the item came back in good condition.
    /// This is the final step - moves status from Returned to Completed.
    /// Only after Completed can the borrower leave a review.
    /// </summary>
    /// <param name="rentalId">The rental to complete</param>
    Task<Rental> CompleteRentalAsync(int rentalId);

    /// <summary>
    /// Gets all rentals where the given user is the borrower.
    /// Used on the Rentals page to show "My Rentals".
    /// </summary>
    /// <param name="renterId">The borrower's user ID</param>
    Task<List<Rental>> GetMyRentalsAsync(int renterId);

    /// <summary>
    /// Gets all rentals for items owned by the given user.
    /// Used on the Rentals page to show "Rentals on My Items".
    /// </summary>
    /// <param name="ownerId">The owner's user ID</param>
    Task<List<Rental>> GetRentalsForMyItemsAsync(int ownerId);

    /// <summary>
    /// Updates an existing item's details. Only the owner should be able to call this.
    /// </summary>
    /// <param name="itemId">The item to update</param>
    /// <param name="title">New title</param>
    /// <param name="description">New description</param>
    /// <param name="dailyRate">New daily rate in pounds</param>
    /// <param name="isAvailable">Whether the item is available for new rentals</param>
    Task<Item> UpdateItemAsync(int itemId, string title, string description, decimal dailyRate, bool isAvailable);

    /// <summary>
    /// Soft deletes an item so it no longer appears in listings.
    /// The API has no DELETE endpoint so this sets IsAvailable to false instead.
    /// </summary>
    /// <param name="itemId">The item to delete</param>
    Task DeleteItemAsync(int itemId);

    /// <summary>
    /// Submits a review for a completed rental. I added a duplicate check so
    /// the same user can't review the same rental twice.
    /// </summary>
    /// <param name="rentalId">The completed rental being reviewed</param>
    /// <param name="reviewerId">The user leaving the review</param>
    /// <param name="rating">Star rating from 1 to 5</param>
    /// <param name="comment">Optional written feedback</param>
    Task<Review> SubmitReviewAsync(int rentalId, int reviewerId, int rating, string comment);

    /// <summary>
    /// Gets all reviews for a specific item. Shown on the item detail page.
    /// </summary>
    /// <param name="itemId">The item to get reviews for</param>
    Task<List<Review>> GetReviewsForItemAsync(int itemId);

    /// <summary>
    /// Calculates the average star rating for an item across all its reviews.
    /// Returns 0 if there are no reviews yet.
    /// </summary>
    /// <param name="itemId">The item to calculate the average for</param>
    Task<double> GetAverageRatingAsync(int itemId);
}
