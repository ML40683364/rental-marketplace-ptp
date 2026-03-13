using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public interface IReviewRepository
{
    Task<List<Review>> GetByRentalAsync(int rentalId);
    Task<List<Review>> GetByItemAsync(int itemId);
    Task<List<Review>> GetByReviewerAsync(int reviewerId);
    Task<double> GetAverageRatingAsync(int itemId);
    Task<Review> CreateAsync(Review review);
    Task<bool> HasReviewedAsync(int rentalId, int reviewerId);
}
