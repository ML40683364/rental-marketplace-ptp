// These tests cover the filtering logic that ViewModels use to search and filter items.
// Like the service tests, no DatabaseFixture is needed here — ItemSearchFilter is pure logic.
using StarterApp.Database.Models;
using StarterApp.Database.Services;

namespace StarterApp.Test.ViewModels;

public class ItemSearchFilterTests
{
    private readonly List<Item> _items =
    [
        new Item { Id = 1, Title = "Electric Drill", Description = "Cordless drill", CategoryId = 1, IsAvailable = true },
        new Item { Id = 2, Title = "Camping Tent", Description = "Waterproof tent", CategoryId = 2, IsAvailable = true },
        new Item { Id = 3, Title = "Board Game", Description = "Fun family game", CategoryId = 3, IsAvailable = true },
    ];

    [Fact]
    public void BySearchTerm_ShouldReturnMatchingItems_WhenSearchTermMatchesTitle()
    {
        // Arrange
        var searchTerm = "drill";

        // Act
        var result = ItemSearchFilter.BySearchTerm(_items, searchTerm).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Electric Drill", result[0].Title);
    }

    [Fact]
    public void BySearchTerm_ShouldReturnAllItems_WhenSearchTermIsEmpty()
    {
        // Arrange
        var searchTerm = string.Empty;

        // Act
        var result = ItemSearchFilter.BySearchTerm(_items, searchTerm).ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ByCategoryId_ShouldReturnOnlyItemsInThatCategory()
    {
        // Arrange — category 2 should only return the Camping Tent
        var categoryId = 2;

        // Act
        var result = ItemSearchFilter.ByCategoryId(_items, categoryId).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Camping Tent", result[0].Title);
    }

    [Fact]
    public void ByCategoryId_ShouldReturnAllItems_WhenCategoryIdIsNull()
    {
        // Arrange
        int? categoryId = null;

        // Act
        var result = ItemSearchFilter.ByCategoryId(_items, categoryId).ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }
}
