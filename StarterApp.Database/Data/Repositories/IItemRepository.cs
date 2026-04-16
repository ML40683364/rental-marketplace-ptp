using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

/// <summary>
/// Repository interface for Items. Inherits the 5 basic database operations
/// from IRepository and adds 3 extra methods that only make sense for items.
/// </summary>
public interface IItemRepository : IRepository<Item>
{
    /// <summary>
    /// Gets all items listed by a specific user.
    /// Used on the profile page to show what someone has listed.
    /// </summary>
    /// <param name="ownerId">The user ID of the item owner</param>
    Task<List<Item>> GetByOwnerAsync(int ownerId);

    /// <summary>
    /// Gets all items in a specific category e.g. all Tools or all Camping gear.
    /// </summary>
    /// <param name="categoryId">The category ID to filter by</param>
    Task<List<Item>> GetByCategoryAsync(int categoryId);

    /// <summary>
    /// Finds items within a certain distance of a GPS location.
    /// Uses the Haversine formula to calculate real-world distance.
    /// This was the most interesting method to implement.
    /// </summary>
    /// <param name="latitude">Centre point latitude</param>
    /// <param name="longitude">Centre point longitude</param>
    /// <param name="radiusKm">How far to search in kilometres</param>
    Task<List<Item>> SearchNearbyAsync(double latitude, double longitude, double radiusKm);
}
