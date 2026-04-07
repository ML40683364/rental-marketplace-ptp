using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

public interface IRentalRepository : IRepository<Rental> // IRentalRepository is inheriting from a generic interface called IRepository<T> for Rental.
{
    Task<List<Rental>> GetByRenterAsync(int renterId);
    Task<List<Rental>> GetByItemAsync(int itemId);
    Task<List<Rental>> GetByStatusAsync(string status);
    Task<Rental> UpdateStatusAsync(int id, string newStatus);
    Task<bool> IsItemAvailableAsync(int itemId, DateTime start, DateTime end);
}
