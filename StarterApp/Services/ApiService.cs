using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StarterApp.Database.Models;

namespace StarterApp.Services;

// This is the HTTP implementation of IApiService.
// Every method here maps to one endpoint on the shared API.
// Base URL is set in MauiProgram.cs so we just use relative paths here.
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Claude generated ApiService with the same if (!response.IsSuccessStatusCode) block copied
    // into all 7 methods, which was a DRY violation — one change would need updating in 7 places.
    // The API docs (section 5) recommended a switch statement to handle 400, 401, 403, 404, and 409
    // separately, so I extracted this helper and replaced all 7 repeated blocks with a single call.
    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        switch (response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                throw new Exception(error?.Message ?? "Validation failed");
            case HttpStatusCode.Unauthorized:
                throw new UnauthorizedAccessException("Authentication required");
            case HttpStatusCode.Forbidden:
                throw new Exception("You do not have permission to perform this action");
            case HttpStatusCode.NotFound:
                throw new Exception("Resource not found");
            case HttpStatusCode.Conflict:
                throw new Exception(error?.Message ?? "Conflict error");
            default:
                throw new Exception(error?.Message ?? $"API error: {response.StatusCode}");
        }
    }

    // grabs the JWT token from secure storage and attaches it to the request
    // any endpoint that needs auth goes through this helper
    private async Task<HttpRequestMessage> CreateAuthenticatedRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        var token = await SecureStorage.GetAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    // --- Authentication ---

    public async Task<User> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("auth/register", new { firstName, lastName, email, password });
        if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);
        return await response.Content.ReadFromJsonAsync<User>();
    }

    // saves the token to secure storage so it persists between app sessions
    public async Task<AuthToken> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("auth/token", new { email, password });
        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Invalid email or password");

        var authToken = await response.Content.ReadFromJsonAsync<AuthToken>();
        // This is stores the JWT token 
        await SecureStorage.SetAsync("auth_token", authToken!.Token);
        return authToken;
    }

    public async Task<User> GetCurrentUserAsync()
    {
        var request = await CreateAuthenticatedRequest(HttpMethod.Get, "users/me");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>();
    }

    public async Task<User> GetUserProfileAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"users/{userId}/profile");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>();
    }

    // --- Items ---

    // HTTP layer for Stage 1 - called by ApiRentalService.GetAvailableItemsAsync()
    // supports optional filtering by category, search text, and page number
    // now returns TotalPages too so the ViewModel can show Next/Previous buttons
    public async Task<(List<Item> Items, int TotalPages)> GetItemsAsync(string? category = null, string? search = null, int page = 1)
    {
        var query = $"items?page={page}";
        if (!string.IsNullOrEmpty(category)) query += $"&category={Uri.EscapeDataString(category)}";
        if (!string.IsNullOrEmpty(search)) query += $"&search={Uri.EscapeDataString(search)}";

        var response = await _httpClient.GetAsync(query);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ItemsResponse>();
        var items = result?.Items.Select(MapToItem).ToList() ?? new List<Item>();

        // the API always returns TotalPages - i was throwing it away before
        // now i pass it back so the ViewModel knows when to disable the Next button
        var totalPages = result?.TotalPages ?? 1;
        return (items, totalPages);
    }

    // Uses PostGIS on the server side - just send coordinates and a radius.
    // Previously the category was added to the URL as a raw string, which would break if a category
    // name contained a space or special character (e.g. "computer games" would confuse the server).
    // Added Uri.EscapeDataString() to match how GetItemsAsync already handles the category parameter,
    // so a space becomes %20 and the API always receives a valid, readable URL.
    public async Task<List<Item>> GetNearbyItemsAsync(double lat, double lon, double radius = 5.0, string category = null)
    {
        var query = $"items/nearby?lat={lat}&lon={lon}&radius={radius}";
        if (!string.IsNullOrEmpty(category)) query += $"&category={Uri.EscapeDataString(category)}";

        var response = await _httpClient.GetAsync(query);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ItemsResponse>();
        return result?.Items.Select(MapToItem).ToList() ?? new List<Item>();
    }

    public async Task<Item> GetItemAsync(int id)
    {
        var response = await _httpClient.GetAsync($"items/{id}");
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<ApiItemDto>();
        return MapToItem(dto!);
    }

    public async Task<Item> CreateItemAsync(CreateItemRequest request)
    {
        var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Post, "items");
        httpRequest.Content = JsonContent.Create(request);
        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);
        var dto = await response.Content.ReadFromJsonAsync<ApiItemDto>();
        return MapToItem(dto!);
    }

    // only the owner can update their own item - the API enforces this with the JWT
    public async Task<Item> UpdateItemAsync(int id, UpdateItemRequest request)
    {
        var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Put, $"items/{id}");
        httpRequest.Content = JsonContent.Create(request);
        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);
        var dto = await response.Content.ReadFromJsonAsync<ApiItemDto>();
        return MapToItem(dto!);
    }

    public async Task DeleteItemAsync(int id)
    {
        var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Delete, $"items/{id}");
        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);
    }

    // --- Categories ---

    // no auth needed, categories are public
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetAsync("categories");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CategoriesResponse>();
        return result?.Categories ?? new List<Category>();
    }

    // --- Rentals ---

    // HTTP layer for Stage 2 - called by ApiRentalService.RequestRentalAsync()
    // dates are sent as yyyy-MM-dd strings as the API expects
    public async Task<Rental> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate)
    {
        var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Post, "rentals");
        httpRequest.Content = JsonContent.Create(new
        {
            itemId,
            startDate = startDate.ToString("yyyy-MM-dd"),
            endDate = endDate.ToString("yyyy-MM-dd")
        });
        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);
        return await response.Content.ReadFromJsonAsync<Rental>();
    }

    // incoming = rentals on items I own (I'm the lender)
    public async Task<List<Rental>> GetIncomingRentalsAsync(string status = null)
    {
        var query = "rentals/incoming";
        if (!string.IsNullOrEmpty(status)) query += $"?status={status}";
        var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Get, query);
        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RentalsResponse>();
        return result?.Rentals ?? new List<Rental>();
    }

    // outgoing = rentals I requested on other people's items (I'm the borrower)
    public async Task<List<Rental>> GetOutgoingRentalsAsync(string status = null)
    {
        var query = "rentals/outgoing";
        if (!string.IsNullOrEmpty(status)) query += $"?status={status}";
        var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Get, query);
        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RentalsResponse>();
        return result?.Rentals ?? new List<Rental>();
    }

    public async Task<Rental> GetRentalAsync(int id)
    {
        var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Get, $"rentals/{id}");
        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Rental>();
    }

    // PATCH is used here because we're only changing one field (status), not the whole rental
    public async Task UpdateRentalStatusAsync(int rentalId, string status)
    {
        var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Patch, $"rentals/{rentalId}/status");
        httpRequest.Content = JsonContent.Create(new { status });
        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);
    }

    // --- Reviews ---

    // HTTP layer for Stage 9 - called by ApiRentalService.SubmitReviewAsync()
    public async Task<Review> CreateReviewAsync(int rentalId, int rating, string comment)
    {
        var httpRequest = await CreateAuthenticatedRequest(HttpMethod.Post, "reviews");
        httpRequest.Content = JsonContent.Create(new { rentalId, rating, comment });
        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);
        return await response.Content.ReadFromJsonAsync<Review>();
    }

    public async Task<List<Review>> GetItemReviewsAsync(int itemId, int page = 1)
    {
        var response = await _httpClient.GetAsync($"items/{itemId}/reviews?page={page}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ReviewsResponse>();
        return result?.Reviews ?? new List<Review>();
    }

    public async Task<List<Review>> GetUserReviewsAsync(int userId, int page = 1)
    {
        var response = await _httpClient.GetAsync($"users/{userId}/reviews?page={page}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ReviewsResponse>();
        return result?.Reviews ?? new List<Review>();
    }

    // --- Response wrapper classes ---
    // The API never returns a plain array - it always wraps it in an object.
    // e.g. GET /items returns { "items": [...], "totalItems": 45 }
    // These private classes match that shape so we can deserialise cleanly.

    // matches each item object inside the API response
    private class ApiItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DailyRate { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public double? OwnerRating { get; set; }
        public bool IsAvailable { get; set; }
        public double? AverageRating { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    // matches the outer wrapper from GET /items and GET /items/nearby
    private class ItemsResponse
    {
        public List<ApiItemDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }

    // converts the API item shape into our local Item model
    private static Item MapToItem(ApiItemDto dto) => new Item
    {
        Id = dto.Id,
        Title = dto.Title,
        Description = dto.Description,
        DailyRate = dto.DailyRate,
        CategoryId = dto.CategoryId,
        OwnerId = dto.OwnerId,
        IsAvailable = dto.IsAvailable,
        CreatedAt = dto.CreatedAt
    };

    private class CategoriesResponse
    {
        public List<Category> Categories { get; set; } = new();
    }

    private class RentalsResponse
    {
        public List<Rental> Rentals { get; set; } = new();
    }

    private class ReviewsResponse
    {
        public List<Review> Reviews { get; set; } = new();
    }

    private class ApiErrorResponse
    {
        public string Error { get; set; }
        public string Message { get; set; }
    }
}
