using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<LocationDetail> LocationDetails { get; set; }
    public DbSet<UserLocation> UserLocations { get; set; }
    public DbSet<PurchaseBill> PurchaseBills { get; set; }
    public DbSet<PurchaseBillItem> PurchaseBillItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed initial data
        modelBuilder.Entity<LocationDetail>().HasData(
            new LocationDetail { Location_Code = "MNG", Location_Name = "Mango" },
            new LocationDetail { Location_Code = "APL", Location_Name = "Apple" },
            new LocationDetail { Location_Code = "BAN", Location_Name = "Banana" },
            new LocationDetail { Location_Code = "ORG", Location_Name = "Orange" },
            new LocationDetail { Location_Code = "GRP", Location_Name = "Grapes" },
            new LocationDetail { Location_Code = "KWI", Location_Name = "Kiwi" },
            new LocationDetail { Location_Code = "STR", Location_Name = "Strawberry" }
        );

        // Seed a test user
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "info@enhanzer.com", Password = "Welcome#3" }
        );
    }
}