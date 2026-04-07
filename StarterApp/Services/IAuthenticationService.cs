// (original) - interface that provides rules 
// handling user authentication and authorization 
// this is for: logging users, registering new users, tracking authentication state, authorization, Handling password changes

using StarterApp.Database.Models;

namespace StarterApp.Services;

public interface IAuthenticationService
{
    event EventHandler<bool>? AuthenticationStateChanged; // changes whenever authentication status changes

    bool IsAuthenticated { get; } // showes here is user is logged in or not. bool - yes/no 
    User? CurrentUser { get; } // has the currently logged-in user
    List<string> CurrentUserRoles { get; } // if the user: Admin, User... 

    Task<AuthenticationResult> LoginAsync(string email, string password);
    Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password); // creates a new user account
    Task LogoutAsync(); // Clears authentication state and logs the user out

    bool HasRole(string roleName); //checks if user has a specific role
    bool HasAnyRole(params string[] roleNames); // returns true if user has at least one of the roles
    bool HasAllRoles(params string[] roleNames); // returns true only if user has every role listed 


    // Password Management
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword); //Changes the user’s password, retuens true if successful, false otherwise
}