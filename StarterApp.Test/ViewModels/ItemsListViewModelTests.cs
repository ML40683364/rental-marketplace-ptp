using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Test.Fixtures;

namespace StarterApp.Test.ViewModels;

public class ItemsListViewModelTests : IDisposable
{
    private readonly DatabaseFixture _fakeDb;
    private readonly ItemRepository _repository;

    public ItemsListViewModelTests()
    {
        _fakeDb = new DatabaseFixture();
        _repository = new ItemRepository(_fakeDb.Context);
    }

    public void Dispose() => _fakeDb.Dispose();

    [Fact]
    public async Task LoadItems_ShouldReturnItems()
    {
        // Act
        var items = await _repository.GetAllAsync();

        // Assert
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task LoadItems_ShouldOnlyShowAvailableItems()
    {
        // Act
        var items = await _repository.GetAllAsync();

        // Assert
        Assert.All(items, item => Assert.True(item.IsAvailable));
    }

    [Fact]
    public async Task LoadItems_ShouldReturnItemWithTitle()
    {
        // Act
        var items = await _repository.GetAllAsync();

        // Assert
        Assert.All(items, item => Assert.False(string.IsNullOrEmpty(item.Title)));
    }

    [Fact]
    public async Task CreateItem_ShouldAppearInList()
    {
        // Arrange
        var newItem = new Item
        {
            Title = "Board Game",
            DailyRate = 3.00m,
            CategoryId = 1,
            OwnerId = 1,
            IsAvailable = true
        };

        // Act
        await _repository.CreateAsync(newItem);
        var items = await _repository.GetAllAsync();

        // Assert
        Assert.Contains(items, i => i.Title == "Board Game");
    }
}
