using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

// Everything a repository needs to work with Items.
// The 5 basics (get, find, create, update, delete) come from IRepository<Item>.
// These 3 below are unique to items — no other repository needs them.
public interface IItemRepository : IRepository<Item> // IItemRepository is inheriting froma generic interface called IRepository<T> for Item.
{
    Task<List<Item>> GetByOwnerAsync(int ownerId);                                       // all items listed by a specific user
    Task<List<Item>> GetByCategoryAsync(int categoryId);                                 // filter by category (tools, camping...)
    Task<List<Item>> SearchNearbyAsync(double latitude, double longitude, double radiusKm); // find items near a location
}
