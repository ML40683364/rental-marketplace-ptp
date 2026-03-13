using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Item>> GetAllAsync()
    {
        return await _context.Items
            .Include(i => i.Owner)
            .Include(i => i.Category)
            .Where(i => i.IsAvailable)
            .ToListAsync();
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _context.Items
            .Include(i => i.Owner)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Item>> GetByOwnerAsync(int ownerId)
    {
        return await _context.Items
            .Include(i => i.Category)
            .Where(i => i.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<List<Item>> GetByCategoryAsync(int categoryId)
    {
        return await _context.Items
            .Include(i => i.Owner)
            .Where(i => i.CategoryId == categoryId && i.IsAvailable)
            .ToListAsync();
    }

    // Haversine formula — calculates distance between two GPS points
    public async Task<List<Item>> SearchNearbyAsync(double latitude, double longitude, double radiusKm)
    {
        var allItems = await _context.Items
            .Include(i => i.Owner)
            .Include(i => i.Category)
            .Where(i => i.IsAvailable && i.Latitude != null && i.Longitude != null)
            .ToListAsync();

        return allItems
            .Where(i => GetDistanceKm(latitude, longitude, i.Latitude!.Value, i.Longitude!.Value) <= radiusKm)
            .ToList();
    }

    public async Task<Item> CreateAsync(Item item)
    {
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<Item> UpdateAsync(Item item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _context.Items.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    // Calculates km between two lat/lng points
    private static double GetDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
