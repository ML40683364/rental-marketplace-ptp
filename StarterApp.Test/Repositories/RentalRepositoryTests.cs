// This file originally had 5 tests that each wrote out a full Rental object inline every time.
// I refactored it by extracting a CreateTestRentalAsync helper so the shared setup lives in one place,
// and added 7 new tests to cover methods that had no coverage — bringing the total from 5 to 12.

using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Test.Fixtures;

namespace StarterApp.Test.Repositories;





// RentalRepositoryTests.cs - refrencing the DatabaseFixture to get access to the fake database context for testing.
public class RentalRepositoryTests : IDisposable
{
    private readonly DatabaseFixture _fakeDb;
    private readonly RentalRepository _repository;

    public RentalRepositoryTests()
    {
        _fakeDb = new DatabaseFixture();
        _repository = new RentalRepository(_fakeDb.Context);
    }

    public void Dispose() => _fakeDb.Dispose();





    // --- Test 1 - this test checks if we can create a rental and save it to the database.

    [Fact]
    public async Task CreateAsync_ShouldSaveRental()
    {
        // Arrange
        var rental = new Rental
        {
            ItemId = 1,
            RenterId = 2,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(3),
            Status = "Requested",
            TotalCost = 10.00m
        };

        // Act
        var saved = await _repository.CreateAsync(rental);

        // Assert
        Assert.True(saved.Id > 0);
        Assert.Equal("Requested", saved.Status);
    }






    // --- Test 2 - this test checks if we can update the status of a rental. 

    //Two steps happen here:
    // 1. First creates a rental with status "Requested"
    // 2. Then changes it to "Approved" and checks it actually changed                                                     
    //This is testing the rental workflow -Requested → Approved.

    [Theory]
    [InlineData("Requested", "Approved")]
    [InlineData("Requested", "Rejected")]
    [InlineData("Approved", "OutForRent")]
    [InlineData("OutForRent", "Returned")]
    [InlineData("Returned", "Completed")]
    public async Task UpdateStatusAsync_ShouldChangeStatus(string fromStatus, string toStatus)
    {
        // Arrange
        var rental = new Rental
        {
            ItemId = 1,
            RenterId = 2,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(7),
            Status = fromStatus,
            TotalCost = 10.00m
        };
        var saved = await _repository.CreateAsync(rental);

        // Act
        var updated = await _repository.UpdateStatusAsync(saved.Id, toStatus);

        // Assert
        Assert.Equal(toStatus, updated.Status);
    }

    // --- Test 3 - this test checks if the IsItemAvailableAsync method correctly identifies when an item is available for certain dates.
    // If no one has booked the item on those dates, is it available?

    [Fact]
    public async Task IsItemAvailableAsync_ShouldReturnTrue_WhenNoDatesOverlap()
    {
        // Act - check dates far in the future with no existing rentals
        var available = await _repository.IsItemAvailableAsync(1, DateTime.Today.AddDays(100), DateTime.Today.AddDays(102));

        // Assert
        Assert.True(available);
    }




    // --- Test 4 - checks double booking prevention
    // Two steps:
    // 1. Books Pink Camping Mattress on days 10-12 with status "Approved"
    // 2. Tries to book the same mattress on the same dates → should return false
    [Fact]
    public async Task IsItemAvailableAsync_ShouldReturnFalse_WhenDatesOverlap()
    {
        // Arrange - create an approved rental for item 2 (Pink Camping Mattress)
        await CreateTestRentalAsync(status: "Approved", daysOffset: 10, itemId: 2);

        // Act - try to book the same item on overlapping dates
        var available = await _repository.IsItemAvailableAsync(2, DateTime.Today.AddDays(10), DateTime.Today.AddDays(12));

        // Assert
        Assert.False(available);
    }





    // --- Test 5 - checks that GetByRenterAsync returns only rentals that belong to the specified renter
    [Fact]
    public async Task GetByRenterAsync_ShouldReturnRentalsForThatRenter()
    {
        // Arrange
        await CreateTestRentalAsync(daysOffset: 20);

        // Act
        var rentals = await _repository.GetByRenterAsync(2);

        // Assert
        Assert.NotEmpty(rentals);
        Assert.All(rentals, r => Assert.Equal(2, r.RenterId));
    }

    // I noticed that before I added this helper, I was writing out the same Rental object
    // over and over again in Tests 4 and 5 and all the new tests below - the same ItemId, RenterId,
    // StartDate, EndDate, Status and TotalCost every single time. That is a DRY violation because
    // if I ever needed to change something (like the RenterId or TotalCost), I would have had to
    // find every single test and update them all one by one. That is how bugs sneak in.
    //
    // The solution was to write this helper method once and call it from every test that needs a rental.
    // Now if something needs to change, I update it in one place and all the tests get the change for free.
    // I got this idea from looking at ReviewRepositoryTests which already had the same pattern -
    // once I understood why it was there I realised I needed the same thing here.
    //
    // I also added three optional parameters so each test can customise just the bits it cares about:
    //   status   - most tests just need "Requested" but the double-booking test needs "Approved"
    //   daysOffset - each test uses different future dates so they do not overlap with each other
    //   itemId   - most tests use item 1 but the double-booking test specifically needs item 2 (Pink Camping Mattress)
    // The default values mean tests that do not care about those details can just call CreateTestRentalAsync()
    // with no arguments and get a sensible rental back without any noise in the test.
    private async Task<Rental> CreateTestRentalAsync(string status = "Requested", int daysOffset = 30, int itemId = 1)
    {
        return await _repository.CreateAsync(new Rental
        {
            ItemId = itemId,
            RenterId = 2,
            StartDate = DateTime.Today.AddDays(daysOffset),
            EndDate = DateTime.Today.AddDays(daysOffset + 2),
            Status = status,
            TotalCost = 10.00m
        });
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRentals()
    {
        // Arrange
        await CreateTestRentalAsync();

        // Act
        var rentals = await _repository.GetAllAsync();

        // Assert
        Assert.NotEmpty(rentals);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectRental()
    {
        // Arrange
        var rental = await CreateTestRentalAsync(daysOffset: 40);

        // Act
        var found = await _repository.GetByIdAsync(rental.Id);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(rental.Id, found.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRentalDoesNotExist()
    {
        // Act
        var found = await _repository.GetByIdAsync(999999);

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public async Task GetByItemAsync_ShouldReturnRentalsForThatItem()
    {
        // Arrange
        await CreateTestRentalAsync(daysOffset: 50);

        // Act
        var rentals = await _repository.GetByItemAsync(1);

        // Assert
        Assert.NotEmpty(rentals);
        Assert.All(rentals, r => Assert.Equal(1, r.ItemId));
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnRentalsWithMatchingStatus()
    {
        // Arrange
        await CreateTestRentalAsync(status: "Approved", daysOffset: 60);

        // Act
        var rentals = await _repository.GetByStatusAsync("Approved");

        // Assert
        Assert.NotEmpty(rentals);
        Assert.All(rentals, r => Assert.Equal("Approved", r.Status));
    }

    [Fact]
    public async Task UpdateAsync_ShouldSaveChanges()
    {
        // Arrange
        var rental = await CreateTestRentalAsync(daysOffset: 70);
        rental.TotalCost = 99.00m;

        // Act
        var updated = await _repository.UpdateAsync(rental);

        // Assert
        Assert.Equal(99.00m, updated.TotalCost);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveRental()
    {
        // Arrange
        var rental = await CreateTestRentalAsync(daysOffset: 80);

        // Act
        await _repository.DeleteAsync(rental.Id);
        var deleted = await _repository.GetByIdAsync(rental.Id);

        // Assert
        Assert.Null(deleted);
    }
}
