namespace StarterApp.Services;

/// <summary>
/// Handles everything related to the device's GPS location.
/// I kept this as an interface so I can mock it in tests
/// without needing a real device with GPS.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets the device's current GPS coordinates.
    /// Returns null if the user denied location permission or GPS is off.
    /// </summary>
    /// <returns>A tuple with Latitude and Longitude, or null if unavailable</returns>
    Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();

    /// <summary>
    /// Checks whether location permission has been granted by the user.
    /// I call this before trying to get location to avoid crashing.
    /// </summary>
    /// <returns>True if location is available, false if permission was denied</returns>
    Task<bool> IsLocationEnabledAsync();

    /// <summary>
    /// Calculates the straight-line distance in kilometres between two GPS points.
    /// Uses the Haversine formula which accounts for the curvature of the earth.
    /// </summary>
    /// <param name="lat1">Latitude of the first point</param>
    /// <param name="lon1">Longitude of the first point</param>
    /// <param name="lat2">Latitude of the second point</param>
    /// <param name="lon2">Longitude of the second point</param>
    /// <returns>Distance in kilometres</returns>
    double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2);
}
