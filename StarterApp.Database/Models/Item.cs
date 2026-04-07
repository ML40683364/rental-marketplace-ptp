using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StarterApp.Database.Models;

// item model - represents an item that can be rented by users. It includes details such as title, description, 
// daily rental rate, location, and availability status. Each item is associated with a category and an owner (user). 
// The model also includes timestamps for when the item was created and last updated. Sets Id as the primary key


[Table("items")]
[PrimaryKey(nameof(Id))]
public class Item
{

    public int Id { get; set; }
    // Title of the item
    [Required]
    public string Title { get; set; } = string.Empty;




    // Description of the item
    public string Description { get; set; } = string.Empty;



    // Daily rental rate for the item
    [Required]
    public decimal DailyRate { get; set; }



    // Category FK (links to Category table)
    public int? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }



    // Location of the item 
    [Required]
    public string Location { get; set; } = string.Empty;



    // For nearby search
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool IsAvailable { get; set; } = true;



    //Timestamps for auditing
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;




    // Foreign key - Foreign key linking the item to a User who owns it
    public int OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public User Owner { get; set; } = null!;
}