//   It tests the pure business rules in RentalValidator — that past start dates are rejected, invalid date ranges are caught, and total cost
//   calculates correctly. No database or fixture needed because RentalValidator is just logic with inputs and outputs.

// Unlike repository tests, these do not need a DatabaseFixture or fake database.
// RentalValidator has no dependencies so each test just calls a method and checks the result.
using StarterApp.Database.Services;

namespace StarterApp.Test.Services;

public class RentalValidatorTests
{
    [Fact]
    public void IsStartDateValid_ShouldReturnFalse_WhenStartDateIsInThePast()
    {
        // Arrange
        var pastDate = DateTime.Today.AddDays(-1);

        // Act
        var result = RentalValidator.IsStartDateValid(pastDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsStartDateValid_ShouldReturnTrue_WhenStartDateIsToday()
    {
        // Arrange
        var today = DateTime.Today;

        // Act
        var result = RentalValidator.IsStartDateValid(today);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidDateRange_ShouldReturnFalse_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var start = DateTime.Today.AddDays(5);
        var end = DateTime.Today.AddDays(2);

        // Act
        var result = RentalValidator.IsValidDateRange(start, end);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CalculateTotalCost_ShouldReturnCorrectAmount()
    {
        // Arrange — 3 days at £10 per day should cost £30
        var start = DateTime.Today;
        var end = DateTime.Today.AddDays(3);
        var dailyRate = 10.00m;

        // Act
        var total = RentalValidator.CalculateTotalCost(start, end, dailyRate);

        // Assert
        Assert.Equal(30.00m, total);
    }
}
