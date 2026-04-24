// fake database

// DatabaseFixture sets up a fake in-memory database for testing.
// Instead of connecting to a real PostgreSQL database, it creates a temporary
// database in memory that only exists while the tests are running.
// This means tests are fast, isolated, and don't affect the real database.

// Microsoft.EntityFrameworkCore.InMemory (Microsoft, 2024) - creates a fake in-memory database for testing
// replaces real PostgreSQL so tests run fast and without any external dependencies
using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;

namespace StarterApp.Test.Fixtures;

public class DatabaseFixture : IDisposable //DatabaseFixture implements the IDisposable interface.
{
    public AppDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        // UseInMemoryDatabase creates a fake database in memory, no PostgreSQL needed
        // Guid.NewGuid() gives each test class its own fresh database so they don't conflict
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
        SeedTestData();  // called here in the constructor to add some starting data to the fake database so tests have something to work with.
    }




    // SeedTestData adds some starting data so tests have something to work with
    private void SeedTestData()
    {
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Tools", Description = "Tools and equipment" },
            new Category { Id = 2, Name = "Camping", Description = "Camping gear" }
        };
        Context.Categories.AddRange(categories);

        var users = new List<User>
        {
            new User { Id = 1, Email = "owner@test.com", FirstName = "Sarah", LastName = "Smith" },
            new User { Id = 2, Email = "renter@test.com", FirstName = "Mike", LastName = "Jones" }
        };
        Context.Users.AddRange(users);

        var items = new List<Item>
        {
            new Item { Id = 1, Title = "Wooden Big Hammer from China", DailyRate = 5.00m, CategoryId = 1, OwnerId = 1, IsAvailable = true, Latitude = 55.9533, Longitude = -3.1883 },
            new Item { Id = 2, Title = "Pink Camping Mattress", DailyRate = 3.00m, CategoryId = 2, OwnerId = 1, IsAvailable = true, Latitude = 55.9600, Longitude = -3.1900 }
        };
        Context.Items.AddRange(items);

        Context.SaveChanges();
    }

    // Dispose cleans up the database after all tests in the class are done
    public void Dispose() // Cleans up the in-memory database after tests are done
    {
        Context.Dispose();
    }
}
