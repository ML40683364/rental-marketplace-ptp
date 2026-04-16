using StarterApp.Database.Models;

namespace StarterApp.Services;

/// <summary>
/// Defines the contract for handling user login, registration and roles.
/// I learned that having this as an interface means I can have two versions -
/// one that talks to the local database and one that calls the API -
/// and the ViewModels don’t need to know which one they’re using.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Fires whenever the user logs in or logs out.
    /// I use this so the UI can react immediately without polling.
    /// </summary>
    event EventHandler<bool>? AuthenticationStateChanged;

    /// <summary>
    /// True if a user is currently logged in, false otherwise.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// The currently logged in user, or null if nobody is logged in.
    /// </summary>
    User? CurrentUser { get; }

    /// <summary>
    /// The list of roles the current user has e.g. "Admin", "User".
    /// Empty list if nobody is logged in.
    /// </summary>
    List<string> CurrentUserRoles { get; }

    /// <summary>
    /// Logs the user in with their email and password.
    /// Returns an AuthenticationResult so I can check if it succeeded
    /// and show the right error message if it didn’t.
    /// </summary>
    /// <param name="email">The user’s email address</param>
    /// <param name="password">The user’s password</param>
    /// <returns>Result indicating success or failure with an error message</returns>
    Task<AuthenticationResult> LoginAsync(string email, string password);

    /// <summary>
    /// Creates a new user account.
    /// I split first and last name into separate fields to match what the API expects.
    /// </summary>
    /// <param name="firstName">User’s first name</param>
    /// <param name="lastName">User’s last name</param>
    /// <param name="email">User’s email address, must be unique</param>
    /// <param name="password">Must meet the API’s password requirements</param>
    /// <returns>Result indicating success or failure with an error message</returns>
    Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password);

    /// <summary>
    /// Logs the user out and clears all stored authentication state.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Checks if the current user has a specific role.
    /// </summary>
    /// <param name="roleName">The role to check for e.g. "Admin"</param>
    /// <returns>True if the user has the role, false otherwise</returns>
    bool HasRole(string roleName);

    /// <summary>
    /// Checks if the current user has at least one of the given roles.
    /// Useful when a feature is available to multiple roles.
    /// </summary>
    /// <param name="roleNames">One or more role names to check</param>
    /// <returns>True if the user has any of the roles</returns>
    bool HasAnyRole(params string[] roleNames);

    /// <summary>
    /// Checks if the current user has every role in the list.
    /// More restrictive than HasAnyRole - all roles must match.
    /// </summary>
    /// <param name="roleNames">All role names the user must have</param>
    /// <returns>True only if the user has every role listed</returns>
    bool HasAllRoles(params string[] roleNames);

    /// <summary>
    /// Changes the user’s password after verifying the current one.
    /// </summary>
    /// <param name="currentPassword">The user’s existing password to verify identity</param>
    /// <param name="newPassword">The new password to set</param>
    /// <returns>True if the password was changed successfully, false otherwise</returns>
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
}