// AppDbContext is the bridge between my C# code and the PostgreSQL database.
// I never write SQL directly - EF Core (Entity Framework Core) translates my C# into SQL for me.
// For example: _context.Items.ToList() becomes SELECT * FROM items automatically.

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data;

public class AppDbContext : DbContext
{
    // empty constructor - used when no options are passed in (e.g. during migrations)
    public AppDbContext()
    { }

    // this constructor is used when the app runs normally via Dependency Injection
    // MauiProgram.cs registers AppDbContext and MAUI passes the options in automatically
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    // this method runs when the app starts up and figures out how to connect to the database
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // if MauiProgram.cs already configured the connection, skip this - no need to do it twice
        if (optionsBuilder.IsConfigured) return;

        // first try to get the connection string from an environment variable
        // (this is how the CI/CD pipeline provides it - keeps passwords out of the code)
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            // no environment variable found - fall back to appsettings.json (local development)
            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("StarterApp.Database.appsettings.json");

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            connectionString = config.GetConnectionString("DevelopmentConnection");
        }

        // tell EF Core to use PostgreSQL with the connection string we found
        optionsBuilder.UseNpgsql(connectionString);
    }

    // each DbSet represents one table in the database
    // e.g. DbSet<Item> Items means there is an "Items" table in PostgreSQL
    // Repositories use these to read and write data: _context.Items.ToList(), _context.Rentals.Add(rental) etc.
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<Review> Reviews { get; set; }

    // OnModelCreating runs once when the app starts and tells EF Core the rules for each table
    // things like: max length of text fields, which fields must be unique, relationships between tables
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User table rules
        // email must be unique - two users cannot share the same email address
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).HasMaxLength(255);
        });

        // Role table rules
        // role name must be unique - cannot have two roles called "Admin"
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // UserRole is a joining table - it links Users to Roles (one user can have many roles)
        // the combination of UserId + RoleId must be unique - a user cannot have the same role twice
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId);
        });

        // Item table rules
        // DailyRate uses precision(10,2) meaning up to 10 digits with 2 decimal places e.g. 99999999.99
        modelBuilder.Entity<Item>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.Property(e => e.DailyRate).HasPrecision(10, 2);
        });

        // Category table rules
        // category name must be unique - cannot have two categories both called "Tools"
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Rental table rules
        // Restrict means: cannot delete an Item or User if they have rentals attached to them
        // this protects data integrity - we don’t want orphaned rentals with no item or renter
        modelBuilder.Entity<Rental>(entity =>
        {
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalCost).HasPrecision(10, 2);

            entity.HasOne(r => r.Item)
                  .WithMany()
                  .HasForeignKey(r => r.ItemId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Renter)
                  .WithMany()
                  .HasForeignKey(r => r.RenterId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Review table rules
        // Cascade means: if a Rental is deleted, all its Reviews are deleted too
        // Restrict means: cannot delete a User if they have written reviews
        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(e => e.Comment).HasMaxLength(1000);

            entity.HasOne(r => r.Rental)
                  .WithMany(rental => rental.Reviews)
                  .HasForeignKey(r => r.RentalId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Reviewer)
                  .WithMany()
                  .HasForeignKey(r => r.ReviewerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

}