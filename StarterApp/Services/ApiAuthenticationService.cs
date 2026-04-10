// i have implemented this part of API from the courswork - API Integration
// This is used in In MauiProgram.cs line 34 - const bool useSharedApi = true;
// so when a user logs in, it hits POST - shared university API and not the local PostgreSQL database

using System.Net.Http.Headers;
using System.Net.Http.Json;
using StarterApp.Database.Models;

namespace StarterApp.Services;


// the class ApiAuthenticationService.cs will follow the rules of the interface IAuthenticationService.cs
public class ApiAuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private User? _currentUser;
    private readonly List<string> _currentUserRoles = new();

    public event EventHandler<bool>? AuthenticationStateChanged;

    public bool IsAuthenticated => _currentUser != null;
    public User? CurrentUser => _currentUser;
    public List<string> CurrentUserRoles => _currentUserRoles;

    public ApiAuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    //  LoginAsync - log in
    // log in - important method in this class, handles the entire login process, communicating with the API, handling the token, and updating the authentication state.

    public async Task<AuthenticationResult> LoginAsync(string email, string password) //login
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/token", new { email, password }); // This sends the email and password to the university API.

            if (!response.IsSuccessStatusCode) // The API sends back a response - either "yes that worked" or "no that failed.
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new AuthenticationResult(false, error?.Message ?? "Login failed");
            }



            // --------------------//
            // If the login worked, the API sends back a token - a long string like

            //  If the login worked, the API sends back a token - a long string like "eyJhbGci...".
            //  this token contains 3 things: actual long string, when it expires, UserId
            //  to check line : private record TokenResponse(string Token, DateTime ExpiresAt, int UserId); 
            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();


            // After getting the token, this attaches it to every future request automatically
            // Every request the app makes to the API after login will automatically include the token in the header.
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token!.Token);




            // After login succeeds and the token is received, the app immediately makes a second call to the API - GET /users/me - to get the logged in user's name, email, ID etc. 

            var meResponse = await _httpClient.GetAsync("users/me"); //gets my profile after login
            var profile = await meResponse.Content.ReadFromJsonAsync<UserProfileResponse>();




            //  This takes the profile that came back from users/me and stores it in memory
            _currentUser = new User
            {
                Id = profile!.Id,
                Email = profile.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                CreatedAt = profile.CreatedAt,
                IsActive = true
            };


            // logged - true / logged out - false
            AuthenticationStateChanged?.Invoke(this, true);
            return new AuthenticationResult(true, "Login successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Login failed: {ex.Message}");
        }
    }





    // RegisterAsync - create account
    // It takes 4 pieces of information: firstName, lastName, email, password — exactly what a user fills in on the registration form.   


    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/register", new
            {
                firstName,
                lastName,
                email,
                password
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new AuthenticationResult(false, error?.Message ?? "Registration failed");
            }

            return new AuthenticationResult(true, "Registration successful. Please log in.");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Registration failed: {ex.Message}");
        }
    }



    // LogoutAsync - log out
    // 3 things - forgets who was logged in, removes the token informes the app that someone just someone just logged out

    public Task LogoutAsync()
    {
        _currentUser = null;
        _currentUserRoles.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = null;
        AuthenticationStateChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    public bool HasRole(string roleName) =>
        _currentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

    public bool HasAnyRole(params string[] roleNames) =>
        roleNames.Any(HasRole);

    public bool HasAllRoles(params string[] roleNames) =>
        roleNames.All(HasRole);

    public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        // Not supported by the shared API
        return Task.FromResult(false);
    }

    // --- API response DTOs ---
    private record TokenResponse(string Token, DateTime ExpiresAt, int UserId);
    private record UserProfileResponse(int Id, string Email, string FirstName, string LastName, DateTime CreatedAt);
    private record ApiErrorResponse(string Error, string Message);
}




