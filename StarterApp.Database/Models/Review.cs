using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StarterApp.Database.Models;

[Table("reviews")]
[PrimaryKey(nameof(Id))]
public class Review
{
    public int Id { get; set; }

    // Which rental this review belongs to
    public int RentalId { get; set; }

    [ForeignKey(nameof(RentalId))]
    public Rental Rental { get; set; } = null!;

    // Who wrote the review
    public int ReviewerId { get; set; }

    [ForeignKey(nameof(ReviewerId))]
    public User Reviewer { get; set; } = null!;

    // 1 to 5 stars
    [Range(1, 5)]
    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    // i added this because the API returns reviewerName as a flat string
    // the Reviewer navigation property is null after deserialisation so
    // Reviewer.FullName would show blank - this captures the name directly
    [NotMapped]
    public string? ReviewerName { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
}
