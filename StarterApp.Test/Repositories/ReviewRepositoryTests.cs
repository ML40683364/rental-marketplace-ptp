// These tests check that ReviewRepository works correctly.
// ReviewRepository handles saving reviews, fetching them, calculating average ratings,
// and checking if someone already left a review for a rental.
// I added these tests because ReviewRepository had zero test coverage and it is
// an important part of the app - users leaving feedback after rentals is a key feature.

// Like the other test files, these use DatabaseFixture which gives a fake in-memory
// database, so no real PostgreSQL connection is needed. Tests are fast and isolated.

// 7 tests in total covering the main methods in ReviewRepository.cs

using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Test.Fixtures;

namespace StarterApp.Test.Repositories;




// ReviewRepositoryTests.cs - referencing DatabaseFixture to get access to the fake
// database context for testing. Same pattern as ItemRepositoryTests and RentalRepositoryTests.
public class ReviewRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fakeDb; // holds the fake database so every test in this class can use it

    public ReviewRepositoryTests(DatabaseFixture fakeDatabase)
    {
        _fakeDb = fakeDatabase;
    }




    // Helper method - I need this because a Review cannot exist without a Rental.
    // Review.RentalId is a foreign key - it must point to a real rental in the database.
    // Instead of copying the rental creation code into every single test,
    // I put it here once and call it from each test that needs it.
    // It creates a completed rental for item 1 (Wooden Big Hammer from China, from seed data).
    private async Task<Rental> CreateTestRentalAsync()
    {
        var rentalRepo = new RentalRepository(_fakeDb.Context);
        return await rentalRepo.CreateAsync(new Rental
        {
            ItemId = 1,       // item 1 exists in seed data (DatabaseFixture.cs)
            RenterId = 2,     // user 2 (Masha) exists in seed data
            StartDate = DateTime.Today.AddDays(-10), // rental happened in the past
            EndDate = DateTime.Today.AddDays(-8),
            Status = "Completed", // only completed rentals should be reviewed
            TotalCost = 10.00m
        });
    }




    // --- Test 1 --- checks that CreateAsync actually saves a review to the database.
    // This is the most basic test - if we cannot save a review, the whole review feature is broken.
    // After saving, I check two things:
    // 1. The database gave it an Id (any number above 0 means it was saved)
    // 2. The rating and comment that came back match what we put in

    [Fact]
    public async Task CreateAsync_ShouldSaveReview()
    {
        // Arrange
        var repository = new ReviewRepository(_fakeDb.Context);
        var rental = await CreateTestRentalAsync(); // need a real rental first because of the foreign key
        var review = new Review
        {
            RentalId = rental.Id,
            ReviewerId = 2,  // Masha (user 2 from seed data) is leaving the review
            Rating = 5,
            Comment = "Great item, would rent again!"
        };

        // Act - this is the actual line being tested
        var saved = await repository.CreateAsync(review);

        // Assert
        Assert.True(saved.Id > 0); // database gave it an Id which means it was saved successfully
        Assert.Equal(5, saved.Rating); // check the rating came back correctly
        Assert.Equal("Great item, would rent again!", saved.Comment); // check the comment came back correctly
    }




    // --- Test 2 --- checks that GetByRentalAsync returns only reviews for a specific rental.
    // If I have rental number 5, and I ask for reviews for rental 5,
    // I should only get back reviews for rental 5, not reviews for other rentals.
    // This matters because on the RentalDetailPage, we only show reviews for that one rental.

    [Fact]
    public async Task GetByRentalAsync_ShouldReturnReviewsForThatRental()
    {
        // Arrange - create a rental and leave a review on it
        var repository = new ReviewRepository(_fakeDb.Context);
        var rental = await CreateTestRentalAsync();
        await repository.CreateAsync(new Review
        {
            RentalId = rental.Id,
            ReviewerId = 2,
            Rating = 4,
            Comment = "Good condition, clean and works well"
        });

        // Act - fetch reviews for that specific rental
        var reviews = await repository.GetByRentalAsync(rental.Id);

        // Assert
        Assert.NotEmpty(reviews); // there should be at least one review
        Assert.All(reviews, r => Assert.Equal(rental.Id, r.RentalId)); // every review must belong to this rental
    }




    // --- Test 3 --- checks that GetByItemAsync returns reviews for a given item.
    // On the ItemDetailPage, we show all reviews that were ever left for that item
    // across all rentals. This test makes sure that works correctly.
    // I create a rental for item 1 and leave a review - then check that item 1 has reviews.

    [Fact]
    public async Task GetByItemAsync_ShouldReturnReviewsForThatItem()
    {
        // Arrange - create a rental for item 1 (Wooden Big Hammer from China) and attach a review
        var repository = new ReviewRepository(_fakeDb.Context);
        var rental = await CreateTestRentalAsync(); // CreateTestRentalAsync always creates a rental for ItemId = 1
        await repository.CreateAsync(new Review
        {
            RentalId = rental.Id,
            ReviewerId = 2,
            Rating = 3,
            Comment = "It was OK, did the job"
        });

        // Act - get all reviews for item 1
        var reviews = await repository.GetByItemAsync(1);

        // Assert - item 1 should have at least the review we just created
        Assert.NotEmpty(reviews);
    }




    // --- Test 4 --- checks that GetAverageRatingAsync returns a positive number when reviews exist.
    // The average rating shows up on the item listing as a star score.
    // I create two rentals for item 1 and leave one 4-star and one 2-star review.
    // The average should be 3.0, but I just check it is above 0 since other tests
    // in this class may have already added reviews for item 1 too.

    [Fact]
    public async Task GetAverageRatingAsync_ShouldReturnPositiveAverage_WhenReviewsExist()
    {
        // Arrange - two rentals, two different ratings
        var repository = new ReviewRepository(_fakeDb.Context);
        var rental1 = await CreateTestRentalAsync();
        var rental2 = await CreateTestRentalAsync();

        await repository.CreateAsync(new Review { RentalId = rental1.Id, ReviewerId = 2, Rating = 4, Comment = "Good" });
        await repository.CreateAsync(new Review { RentalId = rental2.Id, ReviewerId = 2, Rating = 2, Comment = "Average" });

        // Act
        var average = await repository.GetAverageRatingAsync(1); // item 1

        // Assert - since we added reviews, average must be above 0
        Assert.True(average > 0);
    }




    // --- Test 5 --- checks that GetAverageRatingAsync returns exactly 0 when there are no reviews.
    // I use item 999 which does not exist in seed data and has no reviews.
    // If no reviews exist, the method should return 0 instead of crashing.
    // This is an edge case test - what happens when the data is empty.

    [Fact]
    public async Task GetAverageRatingAsync_ShouldReturnZero_WhenNoReviewsExist()
    {
        // Arrange
        var repository = new ReviewRepository(_fakeDb.Context);

        // Act - item 999 has no reviews at all
        var average = await repository.GetAverageRatingAsync(999);

        // Assert
        Assert.Equal(0, average);
    }




    // --- Test 6 --- checks that HasReviewedAsync returns true when a user already left a review.
    // This method is used to prevent duplicate reviews - if Masha already reviewed rental 5,
    // the app should block him from submitting another review for the same rental.
    // Here I create a rental, leave a review, then check that HasReviewedAsync says true.

    [Fact]
    public async Task HasReviewedAsync_ShouldReturnTrue_WhenReviewExists()
    {
        // Arrange - create a rental and leave a review on it
        var repository = new ReviewRepository(_fakeDb.Context);
        var rental = await CreateTestRentalAsync();
        await repository.CreateAsync(new Review
        {
            RentalId = rental.Id,
            ReviewerId = 2, // Masha (user 2) leaves a review
            Rating = 5,
            Comment = "Excellent tool!"
        });

        // Act - ask: has user 2 reviewed this rental?
        var hasReviewed = await repository.HasReviewedAsync(rental.Id, 2);

        // Assert - yes they have, should return true
        Assert.True(hasReviewed);
    }




    // --- Test 7 --- checks that HasReviewedAsync returns false when the user has NOT reviewed yet.
    // Same idea as Test 6 but the opposite case - I create a rental but leave NO review.
    // HasReviewedAsync should return false, meaning the user is allowed to leave a review.
    // This is important because it is what controls whether the review form shows up.

    [Fact]
    public async Task HasReviewedAsync_ShouldReturnFalse_WhenNoReviewExists()
    {
        // Arrange - create a rental but do NOT create any review for it
        var repository = new ReviewRepository(_fakeDb.Context);
        var rental = await CreateTestRentalAsync();

        // Act - ask: has user 2 reviewed this rental? (they have not)
        var hasReviewed = await repository.HasReviewedAsync(rental.Id, 2);

        // Assert - no review was left, should return false
        Assert.False(hasReviewed);
    }
}
