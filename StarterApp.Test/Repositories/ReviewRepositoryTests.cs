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

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllReviews()
    {
        // Arrange
        var rental = await CreateTestRentalAsync();
        await _repository.CreateAsync(new Review { RentalId = rental.Id, ReviewerId = 2, Rating = 4, Comment = "Good condition" });

        // Act
        var reviews = await _repository.GetAllAsync();

        // Assert
        Assert.NotEmpty(reviews);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectReview()
    {
        // Arrange
        var rental = await CreateTestRentalAsync();
        var review = await _repository.CreateAsync(new Review { RentalId = rental.Id, ReviewerId = 2, Rating = 3, Comment = "It was OK" });

        // Act
        var found = await _repository.GetByIdAsync(review.Id);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(review.Id, found.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenReviewDoesNotExist()
    {
        // Act
        var found = await _repository.GetByIdAsync(999999);

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSaveChanges()
    {
        // Arrange
        var rental = await CreateTestRentalAsync();
        var review = await _repository.CreateAsync(new Review { RentalId = rental.Id, ReviewerId = 2, Rating = 3, Comment = "OK" });
        review.Rating = 5;
        review.Comment = "Changed my mind - amazing!";

        // Act
        var updated = await _repository.UpdateAsync(review);

        // Assert
        Assert.Equal(5, updated.Rating);
        Assert.Equal("Changed my mind - amazing!", updated.Comment);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveReview()
    {
        // Arrange
        var rental = await CreateTestRentalAsync();
        var review = await _repository.CreateAsync(new Review { RentalId = rental.Id, ReviewerId = 2, Rating = 4, Comment = "Nice" });

        // Act
        await _repository.DeleteAsync(review.Id);
        var deleted = await _repository.GetByIdAsync(review.Id);

        // Assert
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GetByReviewerAsync_ShouldReturnReviewsByThatUser()
    {
        // Arrange
        var rental = await CreateTestRentalAsync();
        await _repository.CreateAsync(new Review { RentalId = rental.Id, ReviewerId = 2, Rating = 5, Comment = "Fantastic!" });

        // Act
        var reviews = await _repository.GetByReviewerAsync(2);

        // Assert
        Assert.NotEmpty(reviews);
        Assert.All(reviews, r => Assert.Equal(2, r.ReviewerId));
    }
}
