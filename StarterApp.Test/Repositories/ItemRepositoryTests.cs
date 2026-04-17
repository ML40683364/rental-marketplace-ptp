
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
        var items = await _repository.GetAllAsync();
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task GetAllAsync_ShouldOnlyReturnAvailableItems()
    {
        var items = await _repository.GetAllAsync();
        Assert.All(items, item => Assert.True(item.IsAvailable));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectItem()
    {
        var item = await _repository.GetByIdAsync(1);
        Assert.NotNull(item);
        Assert.Equal("Wooden Big Hammer from China", item.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        var item = await _repository.GetByIdAsync(555111);
        Assert.Null(item);
    }

    [Fact]
    public async Task CreateAsync_ShouldSaveNewItem()
    {
        var newItem = new Item
        {
            Title = "Wooden Box from the 18th Century",
            DailyRate = 20.00m,
            CategoryId = 1,
            OwnerId = 1,
            IsAvailable = true
        };

        var savedItem = await _repository.CreateAsync(newItem);
        Assert.True(savedItem.Id > 0);
        Assert.Equal("Wooden Box from the 18th Century", savedItem.Title);
    }

    [Fact]
    public async Task GetByOwnerAsync_ShouldReturnItemsForThatOwner()
    {
        var items = await _repository.GetByOwnerAsync(1);
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.Equal(1, item.OwnerId));
    }
}
