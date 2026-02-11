using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<LocationDetail> LocationDetails { get; set; }
    public DbSet<UserLocation> UserLocations { get; set; }
    public DbSet<PurchaseBill> PurchaseBills { get; set; }
    public DbSet<PurchaseBillItem> PurchaseBillItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Users table
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Configure LocationDetails table
        modelBuilder.Entity<LocationDetail>(entity =>
        {
            entity.ToTable("LocationDetails");
            entity.HasKey(e => e.Location_Code);
            entity.Property(e => e.Location_Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Location_Name).IsRequired().HasMaxLength(100);
        });

        // Configure UserLocations table
        modelBuilder.Entity<UserLocation>(entity =>
        {
            entity.ToTable("UserLocations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Location_Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Location_Name).IsRequired().HasMaxLength(100);
        });

        // Configure PurchaseBills table
        modelBuilder.Entity<PurchaseBill>(entity =>
        {
            entity.ToTable("PurchaseBills");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BatchLocation).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalSelling).HasColumnType("decimal(18,2)");
        });

        // Configure PurchaseBillItems table
        modelBuilder.Entity<PurchaseBillItem>(entity =>
        {
            entity.ToTable("PurchaseBillItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Item).IsRequired().HasMaxLength(200);
            entity.Property(e => e.StandardCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.StandardPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalSelling).HasColumnType("decimal(18,2)");
        });
    }
}