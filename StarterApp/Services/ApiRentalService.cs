using StarterApp.Database.Models;

namespace StarterApp.Services;

// This is the API version of IRentalService - mirrors what RentalService does
// but talks to the shared API instead of the local database.
// Same idea as ApiAuthenticationService vs LocalAuthenticationService.
// MauiProgram.cs decides which one gets injected based on the useSharedApi flag.
public class ApiRentalService : IRentalService
{
    private readonly IApiService _apiService;

    public ApiRentalService(IApiService apiService)
    {
        _apiService = apiService;
    }

    // --- Items ---

    // Stage 1 - Browse Items
    // To demonstrate the full rental workflow I created two test profiles:
    // Mike (the borrower) and Sarah (the owner/lender).
    // This is the first step - Mike opens the app and lands on ItemsListPage.
    // ItemsListViewModel calls this method, which delegates to ApiService.GetItemsAsync()
    // that sends GET /items to the API and returns the list of available items for Mike to browse.
    public async Task<PagedItemsResult> GetAvailableItemsAsync(string? category = null, string? search = null, int page = 1)
    {
        var (items, totalPages) = await _apiService.GetItemsAsync(category, search, page);
        return new PagedItemsResult { Items = items, TotalPages = totalPages, CurrentPage = page };
    }

    public Task<List<Category>> GetCategoriesAsync()
        => _apiService.GetCategoriesAsync();

    public Task<List<Item>> GetNearbyItemsAsync(double latitude, double longitude, double radiusKm = 10)
        => _apiService.GetNearbyItemsAsync(latitude, longitude, radiusKm);

    public Task<Item?> GetItemByIdAsync(int id)
        => _apiService.GetItemAsync(id)!;

    // the local Item model has more fields than the API needs, so we map only what's required
    public async Task<Item> CreateItemAsync(Item item)
    {
        var request = new CreateItemRequest
        {
            Title = item.Title,
            Description = item.Description,
            DailyRate = item.DailyRate,
            CategoryId = item.CategoryId ?? 1,
            Latitude = item.Latitude ?? 0,
            Longitude = item.Longitude ?? 0
        };
        return await _apiService.CreateItemAsync(request);
    }

    public async Task<Item> UpdateItemAsync(int itemId, string title, string description, decimal dailyRate, bool isAvailable)
    {
        var request = new UpdateItemRequest
        {
            Title = title,
            Description = description,
            DailyRate = dailyRate,
            IsAvailable = isAvailable
        };
        return await _apiService.UpdateItemAsync(itemId, request);
    }

    public Task DeleteItemAsync(int itemId)
        => _apiService.DeleteItemAsync(itemId);

    // --- Rentals ---

    // Stage 2 - Create Rental Request
    // Mike (the borrower) has found an item he wants to rent on ItemsListPage.
    // He taps it, lands on ItemDetailPage, picks start and end dates, then taps Request.
    // ItemDetailViewModel calls this method, which delegates to ApiService.RequestRentalAsync()
    // that sends POST /rentals to the API. The rental is created with status "Requested".
    // The renterId param exists in IRentalService for the local DB version but the API
    // doesn't need it - it figures out who the borrower is from the JWT token.
    public Task<Rental> RequestRentalAsync(int itemId, int renterId, DateTime start, DateTime end)
        => _apiService.RequestRentalAsync(itemId, start, end);

    // Stage 3 - Sarah Sees Request
    // Sarah (the owner/lender) opens the app and goes to the Rentals page.
    // RentalsPage.xaml shows her incoming rental requests on items she has listed.
    // RentalsViewModel calls this method, which sends GET /rentals/incoming to the API.
    // The API uses Sarah's JWT token to know whose items to look up - no ownerId needed.
    // The app then shows Approve and Reject buttons for each Requested rental.
    public Task<List<Rental>> GetRentalsForMyItemsAsync(int ownerId)
        => _apiService.GetIncomingRentalsAsync();

    // private helper - avoids repeating the same PATCH + GET pattern in every status method
    private async Task<Rental> UpdateStatusAsync(int rentalId, string status)
    {
        await _apiService.UpdateRentalStatusAsync(rentalId, status);
        return await _apiService.GetRentalAsync(rentalId);
    }

    // Stage 4 - Approve / Reject
    // Sarah (the owner) sees the incoming rental request from Mike on RentalsPage.xaml.
    // She taps either Approve or Reject - RentalsViewModel calls the matching method here.
    // Both send PATCH /rentals/{id}/status to the API via the UpdateStatusAsync helper.
    // If approved, the rental status changes to "Approved" and the app shows the
    // Mark Out for Rent button to Sarah. If rejected, the workflow ends here.
    public Task<Rental> ApproveRentalAsync(int rentalId) => UpdateStatusAsync(rentalId, "Approved");
    public Task<Rental> RejectRentalAsync(int rentalId) => UpdateStatusAsync(rentalId, "Rejected");

    // Stage 5 - Mark Out for Rent
    // After approving, Sarah (the owner) physically hands the item to Mike.
    // She taps the Mark Out for Rent button on RentalsPage.xaml to confirm the handover.
    // RentalsViewModel calls this method, which sends PATCH /rentals/{id}/status to the API.
    // Status moves from "Approved" to "Out for Rent" - the item is now with the borrower.
    public Task<Rental> MarkAsOutForRentAsync(int rentalId) => UpdateStatusAsync(rentalId, "Out for Rent");

    // Stage 6 - Mike Sees Rental
    // Mike (the borrower) opens the app and goes to the Rentals page to check his request.
    // RentalsPage.xaml shows his outgoing rentals - items he has requested from others.
    // RentalsViewModel calls this method, which sends GET /rentals/outgoing to the API.
    // The API uses Mike's JWT token to find his rentals - the renterId param is not needed.
    // Mike sees the rental is now "Out for Rent" and the Mark as Returned button appears.
    public Task<List<Rental>> GetMyRentalsAsync(int renterId)
        => _apiService.GetOutgoingRentalsAsync();

    // Stage 7 - Mark as Returned
    // Mike (the borrower) has finished using the item and returns it to Sarah.
    // He taps the Mark as Returned button on RentalsPage.xaml to confirm the return.
    // RentalsViewModel calls this method, which sends PATCH /rentals/{id}/status to the API.
    // Status moves from "Out for Rent" to "Returned" - Sarah can now confirm on her side.
    public Task<Rental> MarkAsReturnedAsync(int rentalId) => UpdateStatusAsync(rentalId, "Returned");

    // Stage 8 - Complete
    // Sarah (the owner) sees the rental is marked Returned on RentalsPage.xaml.
    // She inspects the item and taps Complete to confirm it was returned in good condition.
    // RentalsViewModel calls this method, which sends PATCH /rentals/{id}/status to the API.
    // Status moves from "Returned" to "Completed" - the full rental cycle is now finished.
    // After this, Mike's RentalsPage shows the Leave Review button to submit feedback.
    public Task<Rental> CompleteRentalAsync(int rentalId) => UpdateStatusAsync(rentalId, "Completed");

    // --- Reviews ---

    // Stage 9 - Leave Review
    // The rental is now Completed. Mike (the borrower) taps Leave Review on RentalsPage.xaml,
    // which navigates him to ReviewsPage.xaml with the rentalId passed as a parameter.
    // ReviewsViewModel calls this method, which delegates to ApiService.CreateReviewAsync()
    // that sends POST /reviews to the API with the rating and comment Mike entered.
    // This is the final step in the full rental workflow: Requested -> Approved ->
    // Out for Rent -> Returned -> Completed -> Review Submitted.
    // reviewerId is not sent - the API infers who is reviewing from the JWT token.
    public Task<Review> SubmitReviewAsync(int rentalId, int reviewerId, int rating, string comment)
        => _apiService.CreateReviewAsync(rentalId, rating, comment);

    public async Task<List<Review>> GetReviewsForItemAsync(int itemId)
        => await _apiService.GetItemReviewsAsync(itemId);

    // the API doesn't have a dedicated average rating endpoint so we calculate it ourselves
    public async Task<double> GetAverageRatingAsync(int itemId)
    {
        var reviews = await _apiService.GetItemReviewsAsync(itemId);
        if (!reviews.Any()) return 0;
        return reviews.Average(r => r.Rating);
    }
}
