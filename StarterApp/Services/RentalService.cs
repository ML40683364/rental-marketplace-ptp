using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;

namespace StarterApp.Services;

// RentalService is the local database implementation of IRentalService.
// It is only used when useSharedApi = false in MauiProgram.cs.
// In API mode, ApiRentalService is used instead and this class is never called.
public class RentalService : IRentalService
{
    private readonly IItemRepository _items;
    private readonly IRentalRepository _rentals;
    private readonly IReviewRepository _reviews;

    // Dependency Injection - Repositories are injected into this Service constructor.
    // MauiProgram.cs registers the repositories and automatically provides them here.
    // This Service never creates repositories itself - it just asks for the interfaces.
    // This means the Service does not care whether data comes from PostgreSQL or anywhere else -
    // it only knows it has something that implements IItemRepository, IRentalRepository, IReviewRepository.
    public RentalService(IItemRepository items, IRentalRepository rentals, IReviewRepository reviews)
    {
        _items = items;
        _rentals = rentals;
        _reviews = reviews;
    }

    // --- Items ---

    // category, search and page are used by the API version
    // local DB just returns everything - TotalPages is 1 since no server-side pagination
    public async Task<PagedItemsResult> GetAvailableItemsAsync(string? category = null, string? search = null, int page = 1)
    {
        var items = await _items.GetAllAsync();
        return new PagedItemsResult { Items = items, TotalPages = 1, CurrentPage = 1 };
    }

    public Task<List<Category>> GetCategoriesAsync()
        => Task.FromResult(new List<Category>());

    public Task<List<Item>> GetNearbyItemsAsync(double latitude, double longitude, double radiusKm = 10)
        => _items.SearchNearbyAsync(latitude, longitude, radiusKm);

    public Task<Item?> GetItemByIdAsync(int id) => _items.GetByIdAsync(id);

    public Task<Item> CreateItemAsync(Item item) => _items.CreateAsync(item);

    public Task DeleteItemAsync(int itemId)
        => _items.DeleteAsync(itemId);

    public async Task<Item> UpdateItemAsync(int itemId, string title, string description, decimal dailyRate, bool isAvailable)
    {
        var item = await _items.GetByIdAsync(itemId)
            ?? throw new Exception("Item not found");
        item.Title = title;
        item.Description = description;
        item.DailyRate = dailyRate;
        item.IsAvailable = isAvailable;
        return await _items.UpdateAsync(item);
    }

    // --- Rentals ---

    public async Task<Rental> RequestRentalAsync(int itemId, int renterId, DateTime start, DateTime end)
    {
        var item = await _items.GetByIdAsync(itemId)
            ?? throw new Exception("Item not found");

        var isAvailable = await _rentals.IsItemAvailableAsync(itemId, start, end);
        if (!isAvailable)
            throw new Exception("Item is not available for the selected dates");

        // Calculate total cost: daily rate x number of days
        var days = Math.Max(1, (end - start).Days);
        var totalCost = item.DailyRate * days;

        var rental = new Rental
        {
            ItemId = itemId,
            RenterId = renterId,
            StartDate = start,
            EndDate = end,
            Status = "Requested",
            TotalCost = totalCost
        };

        return await _rentals.CreateAsync(rental);
    }

    public Task<Rental> ApproveRentalAsync(int rentalId)
        => _rentals.UpdateStatusAsync(rentalId, "Approved");

    public Task<Rental> RejectRentalAsync(int rentalId)
        => _rentals.UpdateStatusAsync(rentalId, "Cancelled");

    public Task<Rental> MarkAsOutForRentAsync(int rentalId)
        => _rentals.UpdateStatusAsync(rentalId, "OutForRent");

    public Task<Rental> MarkAsReturnedAsync(int rentalId)
        => _rentals.UpdateStatusAsync(rentalId, "Returned");

    public Task<Rental> CompleteRentalAsync(int rentalId)
        => _rentals.UpdateStatusAsync(rentalId, "Returned");

    public Task<List<Rental>> GetMyRentalsAsync(int renterId)
        => _rentals.GetByRenterAsync(renterId);

    public async Task<List<Rental>> GetRentalsForMyItemsAsync(int ownerId)
    {
        var myItems = await _items.GetByOwnerAsync(ownerId);
        var result = new List<Rental>();
        foreach (var item in myItems)
        {
            var rentals = await _rentals.GetByItemAsync(item.Id);
            result.AddRange(rentals);
        }
        return result;
    }

    // --- Reviews ---

    public async Task<Review> SubmitReviewAsync(int rentalId, int reviewerId, int rating, string comment)
    {
        var alreadyReviewed = await _reviews.HasReviewedAsync(rentalId, reviewerId);
        if (alreadyReviewed)
            throw new Exception("You have already reviewed this rental");

        var review = new Review
        {
            RentalId = rentalId,
            ReviewerId = reviewerId,
            Rating = rating,
            Comment = comment
        };

        return await _reviews.CreateAsync(review);
    }

    public Task<List<Review>> GetReviewsForItemAsync(int itemId)
        => _reviews.GetByItemAsync(itemId);

    public Task<double> GetAverageRatingAsync(int itemId)
        => _reviews.GetAverageRatingAsync(itemId);
}
