namespace StarterApp.Database.Data.Repositories;

/// <summary>
/// A generic base interface that every repository inherits from.
/// T is a placeholder for the actual model type e.g. Item, Rental, Review.
/// I learned about generics doing this - instead of writing the same 5 methods
/// for every repository, I write them once here and all repos get them for free.
/// </summary>
public interface IRepository<T>
{
    /// <summary>
    /// Returns every record of this type from the database.
    /// </summary>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// Finds a single record by its ID.
    /// Returns null if nothing is found with that ID.
    /// </summary>
    /// <param name="id">The database ID to look up</param>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Saves a new record to the database.
    /// </summary>
    /// <param name="entity">The object to save</param>
    /// <returns>The saved object including its new database ID</returns>
    Task<T> CreateAsync(T entity);

    /// <summary>
    /// Saves changes to an existing record.
    /// </summary>
    /// <param name="entity">The object with updated values</param>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Removes a record from the database by its ID.
    /// </summary>
    /// <param name="id">The ID of the record to delete</param>
    Task DeleteAsync(int id);
}
