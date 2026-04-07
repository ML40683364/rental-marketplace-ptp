using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StarterApp.Database.Models;


// Maps the class to the rentals table in the database
[Table("rentals")]
[PrimaryKey(nameof(Id))]
public class Rental
{
    public int Id { get; set; }

    // Which item is being rented
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item Item { get; set; } = null!;

    // Who is renting it
    public int RenterId { get; set; }

    [ForeignKey(nameof(RenterId))]
    public User Renter { get; set; } = null!;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    // Status: Requested, Approved, OutForRent, Returned, Cancelled
    [Required]
    public string Status { get; set; } = "Requested";

    // Financial Info
    public decimal TotalCost { get; set; }

    // API returns totalPrice instead of totalCost - this maps to that field
    public decimal TotalPrice { get; set; }


    // Timestamps
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    // Reviews
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
