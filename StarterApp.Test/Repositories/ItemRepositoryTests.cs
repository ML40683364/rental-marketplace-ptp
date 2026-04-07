// 6 tests in total, each one checks a different method in ItemRepository.cs. 
// They all use the DatabaseFixture which gives us a fake in-memory database to test against, 
// so we don't need a real PostgreSQL connection. This makes the tests fast and isolated.

// These tests check that ItemRepository correctly saves and fetches items
// from the database. We use the DatabaseFixture which gives us a fake
// in-memory database so we don't need a real PostgreSQL connection.

using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Test.Fixtures;

namespace StarterApp.Test.Repositories;


// Every test class connects to this instead of a real database, so we can test our repository methods without needing 
// real PostgreSQL connection.

//  <DatabaseFixture> means that this test class will use the DatabaseFixture to set up a fake database for testing.
public class ItemRepositoryTests : IClassFixture<DatabaseFixture>
{


    // _fakeDb holds the instance of DatabaseFixture, which gives access to the fake database context for tests.
    private readonly DatabaseFixture _fakeDb;





    // ItemRepositoryTests.cs - refrencing the DatabaseFixture to get access to the fake database context for testing.
    public ItemRepositoryTests(DatabaseFixture fakeDatabase)
    {
        _fakeDb = fakeDatabase;
    }





    // --- TEST 1 --- this test checks that GetAllAsync returns all items where IsAvailable = true.

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllAvailableItems()
    {

        //making this AAA (Arrange, Act, Assert) style for clarity

        // Arrange - create the repository using the fake database
        var repository = new ItemRepository(_fakeDb.Context); // goes straight to this in ItemRepository.cs

        // Act - call the method we are testing
        var items = await repository.GetAllAsync(); // goes straight to this in ItemRepository.cs, which runs a query on the fake database

        // Assert - check we got items back
        Assert.NotEmpty(items);
    }




    // --- TEST 2 --- checks that every item returned has IsAvailable = true, 

    // the difference between this test and the previous one is that this one checks that every item returned has IsAvailable = true, 
    // which is a key part of the GetAllAsync method's functionality. The previous test just checks that we get some items back, 
    // but this one checks that we only get the items that are supposed to be returned according to the method's logic.

    [Fact]
    public async Task GetAllAsync_ShouldOnlyReturnAvailableItems()
    {
        // Arrange
        var repository = new ItemRepository(_fakeDb.Context);

        // Act
        var items = await repository.GetAllAsync();

        // Assert - every item returned should have IsAvailable = true
        Assert.All(items, item => Assert.True(item.IsAvailable)); //  Assert.All means, loop through every item and check that IsAvailable = true. 

    }






    //  --- TEST 3 --- checks that GetByIdAsync returns the correct item when it exists, and null when it doesn't exist.

    //  If I search for Id 1, do I get the Wooden Big Hammer from China? - from data in DatabaseFixture.cs, Id 1 is the Wooden Big Hammer from China, 
    //  It also checks that if we search for an Id that doesn't exist (like 999), we get null back.

    [Fact] // tells xUnit this is a test, when I run the test suite, xUnit finds every method marked with [Fact] and runs it. 
    public async Task GetByIdAsync_ShouldReturnCorrectItem()
    {
        // Arrange
        // Sets up what the test needs. _fakeDb.Context is the fake in-memory database from DatabaseFixture, no real PostgreSQL needed.

        var repository = new ItemRepository(_fakeDb.Context);

        // Act
        // get the item with Id = 1 (Wooden Big Hammer from China from our seed data)
        // the 1 can be any Id that exists in the seed data, it doesn't have to be 1, but i know from 
        // DatabaseFixture.cs that Id 1 is the Wooden Big Hammer from China, so i check for that in the Assert step.

        // Calls the actual method being tested (line that does the real work, everything else supports it)

        var item = await repository.GetByIdAsync(1);

        // Assert

        // Two checks:
        // NotNull — something was actually found, not just an empty                                                                                   
        // Equal — what came back is exactly the right item, not just any item
        Assert.NotNull(item);
        Assert.Equal("Wooden Big Hammer from China", item.Title);
    }







    //  --- TEST 4 --- checks that GetByIdAsync returns null when the item doesn't exist.


    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Arrange
        var repository = new ItemRepository(_fakeDb.Context);

        // Act - try to get an item that doesn't exist
        //  only Id 1 and Id 2 exist. That's why 555111 will never be found and the test passes.
        var item = await repository.GetByIdAsync(555111);

        // Assert
        Assert.Null(item);
    }






    // --- test 5 --- checks that CreateAsync saves a new item to the database.

    [Fact]
    public async Task CreateAsync_ShouldSaveNewItem() // *** Task => Same as async/await in JavaScript - it runs in the background
    {
        // Arrange
        var repository = new ItemRepository(_fakeDb.Context);
        var newItem = new Item
        {
            Title = "Wooden Box from the 18th Century",
            DailyRate = 20.00m,
            CategoryId = 1,
            OwnerId = 1,
            IsAvailable = true
        };

        // Act
        var savedItem = await repository.CreateAsync(newItem); // save the Wooden Box to the fake database, and store the result in savedItem

        // Assert - the item should have been given an Id by the database
        Assert.True(savedItem.Id > 0); //  check that the database gave it an Id (any number above 0 means it was saved successfully) 
        Assert.Equal("Wooden Box from the 18th Century", savedItem.Title); // check that what got saved is actually the Wooden Box and not something else
    }








    // --- test 6 --- checks that GetByOwnerAsync returns only items that belong to the specified owner.

    [Fact]
    public async Task GetByOwnerAsync_ShouldReturnItemsForThatOwner()
    {
        // Arrange
        var repository = new ItemRepository(_fakeDb.Context);

        // Act - get all items owned by user with Id = 1 (Sarah from seed data)
        var items = await repository.GetByOwnerAsync(1);

        // Assert
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.Equal(1, item.OwnerId));
    }
}
