using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

/// <summary>
/// Repository interface for Rentals. Inherits the 5 basic operations
/// from IRepository and adds rental-specific methods for the workflow.
/// </summary>
public interface IRentalRepository : IRepository<Rental>
{
    /// <summary>
    /// Gets all rentals where the given user is the borrower.
    /// </summary>
    /// <param name="renterId">The borrower's user ID</param>
    Task<List<Rental>> GetByRenterAsync(int renterId);

    /// <summary>
    /// Gets all rentals for a specific item across all renters.
    /// </summary>
    /// <param name="itemId">The item ID to look up rentals for</param>
    Task<List<Rental>> GetByItemAsync(int itemId);

    /// <summary>
    /// Gets all rentals with a specific status e.g. all "Requested" ones.
    /// Useful for filtering the rentals list page.
    /// </summary>
    /// <param name="status">Status string e.g. "Requested", "Approved", "Completed"</param>
    Task<List<Rental>> GetByStatusAsync(string status);

    /// <summary>
    /// Changes the status of a rental as it moves through the workflow.
    /// e.g. from Requested to Approved when the owner accepts.
    /// </summary>
    /// <param name="id">The rental ID to update</param>
    /// <param name="newStatus">The new status to set</param>
    Task<Rental> UpdateStatusAsync(int id, string newStatus);

    /// <summary>
    /// Checks whether an item is free for a given date range.
    /// I use this before creating a rental to prevent double booking.
    /// Only Approved and OutForRent rentals count as blocking -
    /// Requested ones don't block the dates yet.
    /// </summary>
    /// <param name="itemId">The item to check</param>
    /// <param name="start">Requested start date</param>
    /// <param name="end">Requested end date</param>
    /// <returns>True if the item is free, false if dates overlap with an existing rental</returns>
    Task<bool> IsItemAvailableAsync(int itemId, DateTime start, DateTime end);
}
