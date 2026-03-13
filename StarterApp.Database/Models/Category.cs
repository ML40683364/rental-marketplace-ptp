using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StarterApp.Database.Models;


// category model - represents a category that items can belong to. It includes a name and description for the category.
[Table("categories")]
[PrimaryKey(nameof(Id))]
public class Category
{
    public int Id { get; set; }


    // Name of the category
    [Required]
    public string Name { get; set; } = string.Empty;


    // Description of the category
    public string Description { get; set; } = string.Empty;

    // Navigation - One category can have many items.   
    public ICollection<Item> Items { get; set; } = new List<Item>();
}
