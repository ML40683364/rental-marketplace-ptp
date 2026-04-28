// ChatGPT generated the initial 6 tests using IClassFixture<DatabaseFixture>, which shares one
// database instance across all tests — I caught this by reading the module notes on determinism,
// which state that tests must produce the same result every time regardless of execution order.
// The AI also repeated var repository = new ItemRepository(_fakeDb.Context) inside every single
// test, so I moved it to a private field in the constructor to fix the DRY violation.
// I replaced IClassFixture with a plain new DatabaseFixture() in the constructor so xUnit creates
// a fresh database for each test, making all 6 tests properly isolated and reliable.

using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Test.Fixtures;
namespace StarterApp.Test.Repositories;

public class ItemRepositoryTests : IDisposable
{
    private readonly DatabaseFixture _fakeDb;
    private readonly ItemRepository _repository;

    public ItemRepositoryTests()
    {
        _fakeDb = new DatabaseFixture();
        _repository = new ItemRepository(_fakeDb.Context);
    }

    public void Dispose() => _fakeDb.Dispose();

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllAvailableItems()
    {
        // Arrange - seed data loaded by DatabaseFixture

        // Act
        var items = await _repository.GetAllAsync();

        // Assert
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task GetAllAsync_ShouldOnlyReturnAvailableItems()
    {
        // Arrange - seed data loaded by DatabaseFixture

        // Act
        var items = await _repository.GetAllAsync();

        // Assert
        Assert.All(items, item => Assert.True(item.IsAvailable));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectItem()
    {
        // Arrange - DatabaseFixture seeded "Wooden Big Hammer from China" with Id 1
        var targetId = 1;

        // Act - call the real method being tested
        var item = await _repository.GetByIdAsync(targetId);

        // Assert - item must exist and must be the correct one, not just any item
        Assert.NotNull(item);
        Assert.Equal("Wooden Big Hammer from China", item.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Arrange - 555111 does not exist in the seed data
        var nonExistentId = 555111;

        // Act
        var item = await _repository.GetByIdAsync(nonExistentId);

        // Assert - nothing should be found
        Assert.Null(item);
    }

    [Fact]
    public async Task CreateAsync_ShouldSaveNewItem()
    {
        // Arrange
        var newItem = new Item
        {
            Title = "Wooden Box from the 18th Century",
            DailyRate = 20.00m,
            CategoryId = 1,
            OwnerId = 1,
            IsAvailable = true
        };

        // Act
        var savedItem = await _repository.CreateAsync(newItem);

        // Assert
        Assert.True(savedItem.Id > 0);
        Assert.Equal("Wooden Box from the 18th Century", savedItem.Title);
    }

    [Fact]
    public async Task GetByOwnerAsync_ShouldReturnItemsForThatOwner()
    {
        // Arrange
        var ownerId = 1;

        // Act
        var items = await _repository.GetByOwnerAsync(ownerId);

        // Assert
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.Equal(ownerId, item.OwnerId));
    }

    [Fact]
    public async Task UpdateAsync_ShouldSaveChanges()
    {
        // Arrange
        var item = await _repository.GetByIdAsync(1);
        item!.Title = "Updated Hammer";
        item.DailyRate = 8.00m;

        // Act
        var updated = await _repository.UpdateAsync(item);

        // Assert
        Assert.Equal("Updated Hammer", updated.Title);
        Assert.Equal(8.00m, updated.DailyRate);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveItem()
    {
        // Arrange - create a disposable item so seed data stays intact
        var item = await _repository.CreateAsync(new Item
        {
            Title = "Item To Delete",
            DailyRate = 1.00m,
            CategoryId = 1,
            OwnerId = 1,
            IsAvailable = true
        });

        // Act
        await _repository.DeleteAsync(item.Id);
        var deleted = await _repository.GetByIdAsync(item.Id);

        // Assert
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnOnlyItemsInThatCategory()
    {
        // Arrange
        var categoryId = 1;

        // Act
        var items = await _repository.GetByCategoryAsync(categoryId);

        // Assert
        Assert.NotEmpty(items);
        Assert.All(items, i => Assert.Equal(categoryId, i.CategoryId));
    }

    [Fact]
    public async Task SearchNearbyAsync_ShouldReturnItemsWithinRadius()
    {
        // Arrange - Edinburgh city centre, both seed items are close by
        var latitude = 55.9533;
        var longitude = -3.1883;
        var radiusKm = 10;

        // Act
        var items = await _repository.SearchNearbyAsync(latitude, longitude, radiusKm);

        // Assert
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task SearchNearbyAsync_ShouldNotReturnItemsOutsideRadius()
    {
        // Arrange - London is over 500km from the Edinburgh seed items
        var latitude = 51.5074;
        var longitude = -0.1278;
        var radiusKm = 1;

        // Act
        var items = await _repository.SearchNearbyAsync(latitude, longitude, radiusKm);

        // Assert
        Assert.Empty(items);
    }
}
