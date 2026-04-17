//   These tests check RentalRepository,  saving rentals, updating status,
//   checking if an item is available for certain dates.
//   use DatabaseFixture to get a fake in-memory database.

using StarterApp.Database.Data.Repositories;  //   to use RentalRepository to test it
using StarterApp.Database.Models;             //   to use Rental and Item models
using StarterApp.Test.Fixtures;               //   to use DatabaseFixture (the fake database) 


// have to use both Rental and Item models 
//    Rental — because we are saving and testing rentals
//    Item — because a rental cannot exist without an item. Every rental must have an ItemId pointing to a real item  in the database 
//    ItemId is the foreign key. It links every rental to a real item in the items table. A rental cannot exist in the database without pointing to a valid item.



// 5 test in total - these tests cover the main functionalities of RentalRepository:

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
        // Arrange - create an approved rental for specific dates
        var rental = new Rental
        {
            ItemId = 2,
            RenterId = 2,
            StartDate = DateTime.Today.AddDays(10),
            EndDate = DateTime.Today.AddDays(12),
            Status = "Approved",
            TotalCost = 30.00m
        };
        await _repository.CreateAsync(rental);

        // Act - try to book the same item on overlapping dates
        var available = await _repository.IsItemAvailableAsync(2, DateTime.Today.AddDays(10), DateTime.Today.AddDays(12));

        // Assert - should not be available
        Assert.False(available);
    }





    // --- Test 5 - checks that GetByRenterAsync returns only rentals that belong to the specified renter
    [Fact]
    public async Task GetByRenterAsync_ShouldReturnRentalsForThatRenter()
    {
        // Arrange - create a rental for renter with Id = 2
        var rental = new Rental
        {
            ItemId = 1,
            RenterId = 2,
            StartDate = DateTime.Today.AddDays(20),
            EndDate = DateTime.Today.AddDays(21),
            Status = "Requested",
            TotalCost = 5.00m
        };
        await _repository.CreateAsync(rental);

        // Act
        var rentals = await _repository.GetByRenterAsync(2);

        // Assert
        Assert.NotEmpty(rentals);
        Assert.All(rentals, r => Assert.Equal(2, r.RenterId));
    }
}
