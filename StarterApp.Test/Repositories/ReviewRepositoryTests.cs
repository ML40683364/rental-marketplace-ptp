using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Test.Fixtures;

namespace StarterApp.Test.Repositories;

public class ReviewRepositoryTests : IDisposable
{
    private readonly DatabaseFixture _fakeDb;
    private readonly ReviewRepository _repository;

    public ReviewRepositoryTests()
    {
        _fakeDb = new DatabaseFixture();
        _repository = new ReviewRepository(_fakeDb.Context);
    }

    public void Dispose() => _fakeDb.Dispose();

    private async Task<Rental> CreateTestRentalAsync()
    {
        var rentalRepo = new RentalRepository(_fakeDb.Context);
        return await rentalRepo.CreateAsync(new Rental
        {
            ItemId = 1,
            RenterId = 2,
            StartDate = DateTime.Today.AddDays(-10),
            EndDate = DateTime.Today.AddDays(-8),
            Status = "Completed",
            TotalCost = 10.00m
        });
    }

    [Fact]
    public async Task CreateAsync_ShouldSaveReview()
    {
        // Arrange
        var rental = await CreateTestRentalAsync();
        var review = new Review
        {
            RentalId = rental.Id,
            ReviewerId = 2,
            Rating = 5,
            Comment = "Great item, would rent again!"
        };

        // Act
        var saved = await _repository.CreateAsync(review);

        // Assert
        Assert.True(saved.Id > 0);
        Assert.Equal(5, saved.Rating);
        Assert.Equal("Great item, would rent again!", saved.Comment);
    }

    [Fact]
    public async Task GetByRentalAsync_ShouldReturnReviewsForThatRental()
    {
        // Arrange
        var rental = await CreateTestRentalAsync();
        await _repository.CreateAsync(new Review
        {
            RentalId = rental.Id,
            ReviewerId = 2,
            Rating = 4,
            Comment = "Good condition, clean and works well"
        });

        // Act
        var reviews = await _repository.GetByRentalAsync(rental.Id);

        // Assert
        Assert.NotEmpty(reviews);
        Assert.All(reviews, r => Assert.Equal(rental.Id, r.RentalId));
    }

    [Fact]
    public async Task GetByItemAsync_ShouldReturnReviewsForThatItem()
    {
        // Arrange
        var rental = await CreateTestRentalAsync();
        await _repository.CreateAsync(new Review
        {
            RentalId = rental.Id,
            ReviewerId = 2,
            Rating = 3,
            Comment = "It was OK, did the job"
        });

        // Act
        var reviews = await _repository.GetByItemAsync(1);

        // Assert
        Assert.NotEmpty(reviews);
    }

    [Fact]
    public async Task GetAverageRatingAsync_ShouldReturnCorrectAverage_WhenReviewsExist()
    {
        // Arrange
        var rental1 = await CreateTestRentalAsync();
        var rental2 = await CreateTestRentalAsync();

        await _repository.CreateAsync(new Review { RentalId = rental1.Id, ReviewerId = 2, Rating = 4, Comment = "Good" });
        await _repository.CreateAsync(new Review { RentalId = rental2.Id, ReviewerId = 2, Rating = 2, Comment = "Average" });

        // Act
        var average = await _repository.GetAverageRatingAsync(1);

        // Assert
        Assert.Equal(3.0, average);
    }

    [Fact]
    public async Task GetAverageRatingAsync_ShouldReturnZero_WhenNoReviewsExist()
    {
        // Act
        var average = await _repository.GetAverageRatingAsync(999);

        // Assert
        Assert.Equal(0, average);
    }

    [Fact]
    public async Task HasReviewedAsync_ShouldReturnTrue_WhenReviewExists()
    {
        // Arrange
        var rental = await CreateTestRentalAsync();
        await _repository.CreateAsync(new Review
        {
            RentalId = rental.Id,
            ReviewerId = 2,
            Rating = 5,
            Comment = "Excellent tool!"
        });

        // Act
        var hasReviewed = await _repository.HasReviewedAsync(rental.Id, 2);

        // Assert
        Assert.True(hasReviewed);
    }

    [Fact]
    public async Task HasReviewedAsync_ShouldReturnFalse_WhenNoReviewExists()
    {
        // Arrange
        var rental = await CreateTestRentalAsync();

        // Act
        var hasReviewed = await _repository.HasReviewedAsync(rental.Id, 2);

        // Assert
        Assert.False(hasReviewed);
    }
}
