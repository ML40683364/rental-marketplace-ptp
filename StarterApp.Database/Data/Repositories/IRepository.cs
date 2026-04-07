namespace StarterApp.Database.Data.Repositories;

// The shared template for all repositories.
// T is a placeholder — when you use it, you swap T for a real type like Item or Rental.
// Every repository gets these 5 basic database operations for free.
public interface IRepository<T>
{
    Task<List<T>> GetAllAsync();        // fetch everything
    Task<T?> GetByIdAsync(int id);      // fetch one by its ID (returns null if not found)
    Task<T> CreateAsync(T entity);      // save a new record
    Task<T> UpdateAsync(T entity);      // save changes to an existing record
    Task DeleteAsync(int id);           // remove a record
}
