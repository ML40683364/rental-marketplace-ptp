namespace StarterApp.Services;

public interface ILocationService
{
    Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();
    Task<bool> IsLocationEnabledAsync();
    double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2);
}
