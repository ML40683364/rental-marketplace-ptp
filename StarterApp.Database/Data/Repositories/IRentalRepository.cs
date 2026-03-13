using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public interface IRentalRepository
{
    Task<List<Rental>> GetAllAsync();
    Task<Rental?> GetByIdAsync(int id);
    Task<List<Rental>> GetByRenterAsync(int renterId);
    Task<List<Rental>> GetByItemAsync(int itemId);
    Task<List<Rental>> GetByStatusAsync(string status);
    Task<Rental> CreateAsync(Rental rental);
    Task<Rental> UpdateStatusAsync(int id, string newStatus);
    Task<bool> IsItemAvailableAsync(int itemId, DateTime start, DateTime end);
}
