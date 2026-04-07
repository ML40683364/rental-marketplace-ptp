// Rental.cs is the data shape of a rental.
// It is just a class that describes what a rental looks like in the database —
// who rented what item, for what dates, what the current status is, and how much it costs.
// EF Core uses this class to create and manage the rentals table in PostgreSQL.
// Every property here becomes a column in that table.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StarterApp.Database.Models;


// [Table("rentals")] tells EF Core the exact table name in PostgreSQL.
// Without this, EF Core would guess the name and might get it wrong.
[Table("rentals")]
[PrimaryKey(nameof(Id))]
public class Rental
{
    // Every rental gets a unique Id automatically assigned by the database.
    // This is how we find a specific rental later - like a receipt number.
    public int Id { get; set; }

    // ItemId is a foreign key - it links this rental to a specific item in the items table.
    // A rental cannot exist without an item, so this is required.
    public int ItemId { get; set; }

    // Item is the navigation property - it lets us access the full Item object
    // from a Rental without writing a separate database query.
    // The = null! tells the compiler "I know this looks null but EF Core will fill it in".
    // I was confused by this at first but it is just a way to avoid compiler warnings.
    [ForeignKey(nameof(ItemId))]
    public Item Item { get; set; } = null!;

    // RenterId links this rental to the user who is renting the item.
    // Same pattern as ItemId above - foreign key + navigation property.
    public int RenterId { get; set; }

    [ForeignKey(nameof(RenterId))]
    public User Renter { get; set; } = null!;

    // StartDate and EndDate define how long the rental lasts.
    // [Required] means EF Core will not let us save a rental without these dates.
    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    // Status tracks where the rental is in its lifecycle.
    // It starts as "Requested" and moves through:
    // Requested → Approved → OutForRent → Returned → Completed
    // Or it can be Rejected or Cancelled if things go wrong.
    // I learned that managing these transitions carefully is called a state machine.
    [Required]
    public string Status { get; set; } = "Requested";

    // TotalCost is used by the local database version of the app.
    // TotalPrice is what the shared API returns - the field name is different on the API side.
    // I had to add TotalPrice because the API response was not mapping correctly without it.
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }

    // CreatedAt and UpdatedAt track when the rental was created and last changed.
    // The ? means they are nullable - they do not have to be set manually because
    // they default to the current time automatically.
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    // Reviews is a collection of all reviews left for this rental.
    // One rental can have many reviews - this is a one-to-many relationship.
    // ICollection is used instead of List because EF Core works better with it for navigation.
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
