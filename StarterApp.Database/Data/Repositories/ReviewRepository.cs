using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Review>> GetAllAsync()
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .ToListAsync();
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Review> UpdateAsync(Review review)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task DeleteAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review != null)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Review>> GetByRentalAsync(int rentalId)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.RentalId == rentalId)
            .ToListAsync();
    }

    public async Task<List<Review>> GetByItemAsync(int itemId)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Rental)
            .Where(r => r.Rental.ItemId == itemId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Review>> GetByReviewerAsync(int reviewerId)
    {
        return await _context.Reviews
            .Include(r => r.Rental)
            .Where(r => r.ReviewerId == reviewerId)
            .ToListAsync();
    }

    // Returns average star rating for an item (e.g. 4.5)
    public async Task<double> GetAverageRatingAsync(int itemId)
    {
        var ratings = await _context.Reviews
            .Where(r => r.Rental.ItemId == itemId)
            .Select(r => r.Rating)
            .ToListAsync();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }

    public async Task<Review> CreateAsync(Review review)
    {
        review.CreatedAt = DateTime.UtcNow;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    // Prevents a user from reviewing the same rental twice
    public async Task<bool> HasReviewedAsync(int rentalId, int reviewerId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.RentalId == rentalId && r.ReviewerId == reviewerId);
    }
}
