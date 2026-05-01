using System.Globalization;

namespace StarterApp.Converters;

/// <summary>
/// Converts a rental status string to a colour so each status is visually distinct.
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string status ? status switch
        {
            "Requested"   => Color.FromArgb("#FF9800"), // orange  - waiting for owner
            "Approved"    => Color.FromArgb("#2196F3"), // blue    - confirmed
            "Out for Rent"=> Color.FromArgb("#FF9800"), // orange  - in progress
            "Returned"    => Color.FromArgb("#2196F3"), // blue    - back with owner
            "Completed"   => Color.FromArgb("#4CAF50"), // green   - all done
            "Rejected"    => Color.FromArgb("#F44336"), // red     - declined
            _             => Color.FromArgb("#512BD4"), // purple  - default/unknown
        } : Color.FromArgb("#512BD4");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
