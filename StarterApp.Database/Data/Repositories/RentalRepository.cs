using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rental>> GetAllAsync()
    {
        return await _context.Rentals
            .Include(r => r.Item)
            .Include(r => r.Renter)
            .ToListAsync();
    }

    public async Task<Rental?> GetByIdAsync(int id)
    {
        return await _context.Rentals
            .Include(r => r.Item)
            .Include(r => r.Renter)
            .Include(r => r.Reviews)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Rental>> GetByRenterAsync(int renterId)
    {
        return await _context.Rentals
            .Include(r => r.Item)
            .Where(r => r.RenterId == renterId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Rental>> GetByItemAsync(int itemId)
    {
        return await _context.Rentals
            .Include(r => r.Renter)
            .Where(r => r.ItemId == itemId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Rental>> GetByStatusAsync(string status)
    {
        return await _context.Rentals
            .Include(r => r.Item)
            .Include(r => r.Renter)
            .Where(r => r.Status == status)
            .ToListAsync();
    }


    //  Test 1 using this method to create a rental and save it to the database.
    //  Test 2 using this method to create a rental first before updating its status.
    //  Test 4 using this method to create an approved rental to test date overlap.
    public async Task<Rental> CreateAsync(Rental rental)  // 1 + 2 
    {
        rental.CreatedAt = DateTime.UtcNow;
        rental.UpdatedAt = DateTime.UtcNow;
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();
        return rental;
    }

    public async Task<Rental> UpdateAsync(Rental rental)
    {
        rental.UpdatedAt = DateTime.UtcNow;
        _context.Rentals.Update(rental);
        await _context.SaveChangesAsync();
        return rental;
    }

    public async Task DeleteAsync(int id)
    {
        var rental = await _context.Rentals.FindAsync(id);
        if (rental != null)
        {
            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Rental> UpdateStatusAsync(int id, string newStatus)
    {
        var rental = await _context.Rentals.FindAsync(id)
            ?? throw new KeyNotFoundException($"Rental {id} not found");

        rental.Status = newStatus;
        rental.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return rental;
    }

    // Checks no approved/active rental overlaps the requested dates

    // Test 3 using this method to check if an item is available for certain dates.
    //  Test 4 using this method to check if an item is available for certain dates when there is an overlapping rental.
    public async Task<bool> IsItemAvailableAsync(int itemId, DateTime start, DateTime end)
    {
        var conflict = await _context.Rentals
            .Where(r => r.ItemId == itemId
                && (r.Status == "Approved" || r.Status == "OutForRent")
                && r.StartDate < end
                && r.EndDate > start)
            .AnyAsync();

        return !conflict;
    }
}
