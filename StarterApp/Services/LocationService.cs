namespace StarterApp.Services;

public class LocationService : ILocationService
{
    // Asks the device for current GPS position
    public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return null;

            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location == null) return null;

            return (location.Latitude, location.Longitude);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsLocationEnabledAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        return status == PermissionStatus.Granted;
    }

    // Same Haversine formula as ItemRepository — calculates km between two GPS points
    public double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
