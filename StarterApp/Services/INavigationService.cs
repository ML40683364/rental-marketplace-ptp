namespace StarterApp.Services;

/// <summary>
/// Handles moving between pages in the app.
/// I wrapped MAUI's built in navigation in this interface so ViewModels
/// don't talk to Shell directly - I learned this makes them easier to test.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to a page by its route name e.g. "ItemsListPage".
    /// </summary>
    /// <param name="route">The route string registered in AppShell</param>
    Task NavigateToAsync(string route);

    /// <summary>
    /// Navigates to a page and passes data to it at the same time.
    /// I use this when I need to send something like an item ID to the next page.
    /// </summary>
    /// <param name="route">The route string registered in AppShell</param>
    /// <param name="parameters">Key-value pairs to pass to the destination page</param>
    Task NavigateToAsync(string route, Dictionary<string, object> parameters);

    /// <summary>
    /// Goes back one page, like pressing the back button.
    /// </summary>
    Task NavigateBackAsync();

    /// <summary>
    /// Jumps all the way back to the first page in the navigation stack.
    /// I use this after logout so the user can't press back to get back in.
    /// </summary>
    Task NavigateToRootAsync();

    /// <summary>
    /// Same as NavigateToRootAsync - clears the whole navigation stack.
    /// I kept both because different parts of the app were already using each one.
    /// </summary>
    Task PopToRootAsync();
}