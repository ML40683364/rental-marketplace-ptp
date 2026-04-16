using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

/// <summary>
/// Repository interface for Reviews. Inherits the 5 basic operations
/// from IRepository and adds review-specific methods.
/// </summary>
public interface IReviewRepository : IRepository<Review>
{
    /// <summary>
    /// Gets all reviews linked to a specific rental.
    /// </summary>
    /// <param name="rentalId">The rental ID to get reviews for</param>
    Task<List<Review>> GetByRentalAsync(int rentalId);

    /// <summary>
    /// Gets all reviews for a specific item across all its rentals.
    /// Shown on the item detail page.
    /// </summary>
    /// <param name="itemId">The item ID to get reviews for</param>
    Task<List<Review>> GetByItemAsync(int itemId);

    /// <summary>
    /// Gets all reviews written by a specific user.
    /// </summary>
    /// <param name="reviewerId">The user ID of the reviewer</param>
    Task<List<Review>> GetByReviewerAsync(int reviewerId);

    /// <summary>
    /// Calculates the average star rating for an item across all its reviews.
    /// Returns 0 if there are no reviews yet.
    /// </summary>
    /// <param name="itemId">The item to calculate the average for</param>
    /// <returns>Average rating e.g. 4.5</returns>
    Task<double> GetAverageRatingAsync(int itemId);

    /// <summary>
    /// Checks if a user has already reviewed a specific rental.
    /// I use this to prevent the same person submitting two reviews for one rental.
    /// </summary>
    /// <param name="rentalId">The rental to check</param>
    /// <param name="reviewerId">The user to check</param>
    /// <returns>True if they already left a review, false if they haven't</returns>
    Task<bool> HasReviewedAsync(int rentalId, int reviewerId);
}
