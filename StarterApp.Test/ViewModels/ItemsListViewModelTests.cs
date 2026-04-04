// ItemsListViewModel loads items from ItemRepository and shows them on screen.
// Since we cannot reference the MAUI project directly in tests, we test, It is the only one with that problem. 
// what the ViewModel depends on — ItemRepository.
// If the repository works correctly, the ViewModel will work correctly too.


//   This file tests the most basic things that need to work for the items list screen. 
//   The items list page is the first thing a user sees when they open the marketplace. If it is broken, nothing works.
//   So I wrote 4 simple checks:
//   1. First I check that items actually load — because if the list is empty the whole page is useless. 
//   2. Then I check that only available items show up — because if sold or unavailable items show up, users would get confused and frustrated.
//   3. Then I check that every item has a title — because if an item doesn't have a title, it won't display correctly on the screen.
//   4. Finally I check that when a user creates a new item, it actually shows up in the list — because if the

using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Test.Fixtures;

namespace StarterApp.Test.ViewModels;

public class ItemsListViewModelTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fakeDb;

    public ItemsListViewModelTests(DatabaseFixture fakeDatabase)
    {
        _fakeDb = fakeDatabase;
    }







    [Fact]
    // Test 1 — items load at all??
    // Checks that when open the items list page, actually get some items back. Not an empty screen.
    // The whole point of ItemsListPage is to show items. If nothing loads, the page is completely broken. So the first  
    //  thing to check is, does it return anything at all?
    public async Task LoadItems_ShouldReturnItems()
    {
        // Arrange - this is what ItemsListViewModel calls to load items
        var repository = new ItemRepository(_fakeDb.Context);

        // Act
        var items = await repository.GetAllAsync();

        // Assert - the list should not be empty
        Assert.NotEmpty(items);
    }








    [Fact]
    // Test 2 — LoadItems_ShouldOnlyShowAvailableItems                                                                   
    // Checks that only available items show up. If an item is not available it should not appear in the list.
    // In ItemRepository.GetAllAsync() there is a .Where(i => i.IsAvailable) filter. If that filter breaks, sold or      
    // unavailable items would show up on the list page. This test makes sure that filter is working.
    public async Task LoadItems_ShouldOnlyShowAvailableItems()
    {
        // Arrange
        var repository = new ItemRepository(_fakeDb.Context);

        // Act
        var items = await repository.GetAllAsync();

        // Assert - ItemsListPage should only show available items
        Assert.All(items, item => Assert.True(item.IsAvailable));
    }








    [Fact]
    //  Test 3 — LoadItems_ShouldReturnItemWithTitle                                                                    
    // Checks that every item has a title. If an item doesn't have a title, it won't display correctly on the screen. This test ensures that all items have a title.
    //  The items list page displays the title of each item. If a title is empty, the user sees a blank row on screen.
    // This test makes sure that never happens.
    public async Task LoadItems_ShouldReturnItemWithTitle()
    {
        // Arrange
        var repository = new ItemRepository(_fakeDb.Context);

        // Act
        var items = await repository.GetAllAsync();

        // Assert - every item must have a title so it displays correctly on screen
        Assert.All(items, item => Assert.False(string.IsNullOrEmpty(item.Title)));
    }






    [Fact]
    //Test 4 — CreateItem_ShouldAppearInList
    // Checks that after a user creates a new item called "Board Game", it actually shows up in the list.
    //  This tests the full flow — a user goes to CreateItemPage, fills in the form and saves. Then goes back to
    // ItemsListPage. The new item must show up. If CreateAsync is broken, the item would never appear.
    public async Task CreateItem_ShouldAppearInList()
    {
        // Arrange - simulate a user creating a new item on CreateItemPage
        var repository = new ItemRepository(_fakeDb.Context);
        var newItem = new Item
        {
            Title = "Board Game",
            DailyRate = 3.00m,
            CategoryId = 1,
            OwnerId = 1,
            IsAvailable = true
        };

        // Act - save it then load the list
        await repository.CreateAsync(newItem);
        var items = await repository.GetAllAsync();

        // Assert - the new item should now appear in the list
        Assert.Contains(items, i => i.Title == "Board Game");
    }
}
