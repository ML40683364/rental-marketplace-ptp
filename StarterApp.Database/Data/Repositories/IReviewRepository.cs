using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

// Everything a repository needs to work with Reviews.
// The 5 basics come from IRepository<Review>.
// These below are unique to reviews.
public interface IReviewRepository : IRepository<Review> // IReviewRepository is inheriting from a generic interface called IRepository<T> for Review.
{
    Task<List<Review>> GetByRentalAsync(int rentalId);      // all reviews for a specific rental
    Task<List<Review>> GetByItemAsync(int itemId);          // all reviews for a specific item
    Task<List<Review>> GetByReviewerAsync(int reviewerId);  // all reviews written by a user
    Task<double> GetAverageRatingAsync(int itemId);         // e.g. returns 4.5 stars
    Task<bool> HasReviewedAsync(int rentalId, int reviewerId); // stops someone reviewing twice
}
