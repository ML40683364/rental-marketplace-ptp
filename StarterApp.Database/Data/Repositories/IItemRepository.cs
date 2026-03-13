using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public interface IItemRepository
{
    Task<List<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(int id);
    Task<List<Item>> GetByOwnerAsync(int ownerId);
    Task<List<Item>> GetByCategoryAsync(int categoryId);
    Task<List<Item>> SearchNearbyAsync(double latitude, double longitude, double radiusKm);
    Task<Item> CreateAsync(Item item);
    Task<Item> UpdateAsync(Item item);
    Task DeleteAsync(int id);
}
