using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarterApp.Database.Models;

[Table("users")]
[PrimaryKey(nameof(Id))]
public class User
{
    public int Id { get; set; }
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    [Required]
    public string PasswordSalt { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Many-to-many: A user can have multiple roles (e.g., Admin, Renter, Owner).
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // One-to-many: A user owns multiple items.
    public ICollection<Item> Items { get; set; } = new List<Item>();

    // One-to-many: A user can have multiple rentals (as a renter).
    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();

    // One-to-many: A user can write multiple reviews.
    public ICollection<Review> Reviews { get; set; } = new List<Review>();



    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}