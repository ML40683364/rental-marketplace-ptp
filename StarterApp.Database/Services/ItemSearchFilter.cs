using StarterApp.Database.Models;

namespace StarterApp.Database.Services;

// Logic for filtering a list of items by search term or category.
// I put this here so ViewModels can use it without containing the filtering logic themselves —
// keeping ViewModels thin is part of the MVVM pattern.
// Like RentalValidator, it has no database dependency so it is straightforward to test.
public static class ItemSearchFilter
{
    // Returns only items whose title or description contains the search term (case-insensitive).
    // Returns all items if the search term is empty.
    public static IEnumerable<Item> BySearchTerm(IEnumerable<Item> items, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return items;

        return items.Where(i =>
            i.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            i.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    // Returns only items that belong to the given category.
    // Returns all items if categoryId is null.
    public static IEnumerable<Item> ByCategoryId(IEnumerable<Item> items, int? categoryId)
    {
        if (categoryId is null)
            return items;

        return items.Where(i => i.CategoryId == categoryId);
    }
}
