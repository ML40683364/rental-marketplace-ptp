// ReviewRepository.cs handles all database operations for reviews.
// This is the file that actually talks to PostgreSQL - saving reviews, fetching them,
// calculating averages and checking for duplicates.
// It follows the Repository Pattern which means the ViewModels never talk to the
// database directly, they always go through here instead.
// I implemented IReviewRepository so the ViewModel only depends on the interface,
// not this specific class - that makes it easier to test and swap out later.

using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public class ReviewRepository : IReviewRepository
{
    // _context is the connection to the database.
    // AppDbContext is the EF Core class that knows about all our tables (Reviews, Items, Rentals etc.)
    // It gets injected in the constructor - I don't create it myself, the DI system handles that.
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }




    // GetAllAsync - fetches every review in the database.
    // .Include(r => r.Reviewer) loads the User who wrote the review alongside it.
    // Without Include, Reviewer would be null even though the data exists - EF Core
    // does not load related data automatically unless you ask for it.
    public async Task<List<Review>> GetAllAsync()
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .ToListAsync();
    }




    // GetByIdAsync - fetches one specific review by its Id.
    // Returns null if no review with that Id exists - that is why the return type is Review?
    // The ? means it can be null. I learned this is called a nullable return type.
    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .FirstOrDefaultAsync(r => r.Id == id);
    }




    // UpdateAsync - saves changes to an existing review.
    // _context.Reviews.Update() tells EF Core that this review has changed.
    // SaveChangesAsync() is what actually writes the change to PostgreSQL.
    // Without SaveChangesAsync, nothing gets persisted - I made this mistake early on.
    public async Task<Review> UpdateAsync(Review review)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
        return review;
    }




    // DeleteAsync - removes a review from the database by Id.
    // I first fetch the review to make sure it actually exists before trying to delete it.
    // If I tried to remove something that does not exist, EF Core would throw an error.
    // The null check prevents that from happening.
    public async Task DeleteAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review != null)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }




    // GetByRentalAsync - fetches all reviews that belong to a specific rental.
    // Used on the rental detail screen to show what was said about that particular rental.
    // .Where(r => r.RentalId == rentalId) filters down to only the reviews we want.
    public async Task<List<Review>> GetByRentalAsync(int rentalId)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.RentalId == rentalId)
            .ToListAsync();
    }




    // GetByItemAsync - fetches all reviews ever left for a specific item, across all rentals.
    // This is what shows up on the ItemDetailPage - every review for that item from any rental.
    // I need to Include(r => r.Rental) here so I can navigate from Review → Rental → ItemId.
    // OrderByDescending shows the newest reviews first which is better UX.
    public async Task<List<Review>> GetByItemAsync(int itemId)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Rental)
            .Where(r => r.Rental.ItemId == itemId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }




    // GetByReviewerAsync - fetches all reviews written by a specific user.
    // Could be useful for a "my reviews" profile page in the future.
    public async Task<List<Review>> GetByReviewerAsync(int reviewerId)
    {
        return await _context.Reviews
            .Include(r => r.Rental)
            .Where(r => r.ReviewerId == reviewerId)
            .ToListAsync();
    }




    // GetAverageRatingAsync - calculates the average star rating for an item.
    // First fetches all ratings for that item, then uses .Average() to get the mean.
    // If there are no reviews at all, it returns 0 instead of crashing.
    // The ternary operator (? :) handles that edge case in one line.
    // Example: ratings of 4 and 2 → average is 3.0
    public async Task<double> GetAverageRatingAsync(int itemId)
    {
        var ratings = await _context.Reviews
            .Where(r => r.Rental.ItemId == itemId)
            .Select(r => r.Rating)
            .ToListAsync();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }




    // CreateAsync - saves a brand new review to the database.
    // I set CreatedAt here automatically so the caller does not have to worry about it.
    // This is the method called when a user submits a review on ReviewsPage.
    public async Task<Review> CreateAsync(Review review)
    {
        review.CreatedAt = DateTime.UtcNow;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }




    // HasReviewedAsync - checks if a user already reviewed a specific rental.
    // This prevents duplicate reviews - if someone already reviewed rental 5,
    // they should not be able to submit another one for the same rental.
    // AnyAsync returns true if at least one matching review exists, false if none do.
    public async Task<bool> HasReviewedAsync(int rentalId, int reviewerId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.RentalId == rentalId && r.ReviewerId == reviewerId);
    }
}
