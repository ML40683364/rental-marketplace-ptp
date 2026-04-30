// This file had to be created because RentalService targets Android and the test project cannot reference it directly. I extracted the
// pure business rules into RentalValidator inside the database library — which the test project can already reach — so I could test       
// service logic without restructuring the project close to submission.


namespace StarterApp.Database.Services;

// Unlike repositories, this class has no database or dependencies — just pure business rules.
// I put it here in StarterApp.Database so the test project can reach it,
// because RentalService lives in the MAUI project which tests cannot reference.
public static class RentalValidator
{
    // Start date must be today or in the future
    public static bool IsStartDateValid(DateTime startDate)
        => startDate.Date >= DateTime.Today;

    // End date must be after start date — same-day rentals are not allowed
    public static bool IsValidDateRange(DateTime startDate, DateTime endDate)
        => endDate.Date > startDate.Date;

    // Total cost = number of days × daily rate
    public static decimal CalculateTotalCost(DateTime startDate, DateTime endDate, decimal dailyRate)
    {
        var days = (int)Math.Ceiling((endDate.Date - startDate.Date).TotalDays);
        return days * dailyRate;
    }
}
