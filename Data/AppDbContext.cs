using Microsoft.EntityFrameworkCore;
using Proje.Models;

namespace Proje.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<StockTransaction> StockTransactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Elektronik", Description = "Kablolar, donanımlar ve aksesuarlar" },
            new Category { Id = 2, Name = "Gıda", Description = "Ofis içi ikramlar ve kuru gıdalar" },
            new Category { Id = 3, Name = "Yedek Parça", Description = "Makine yedek parçaları, somun, cıvata vb." },
            new Category { Id = 4, Name = "Ofis Malzemeleri", Description = "Kırtasiye ve sarf malzemeleri" }
        );

        // Seed Products
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Sku = "ELK-001",
                Name = "Kablosuz Mouse",
                CategoryId = 1,
                Quantity = 15,
                Unit = "Adet",
                Location = "A-Rafı 1",
                PurchasePrice = 150.00m,
                SalePrice = 250.00m,
                MinStockLevel = 5,
                CreatedAt = new DateTime(2026, 5, 1, 10, 0, 0),
                UpdatedAt = new DateTime(2026, 5, 1, 10, 0, 0)
            },
            new Product
            {
                Id = 2,
                Sku = "ELK-002",
                Name = "HDMI Kablosu 1.5m",
                CategoryId = 1,
                Quantity = 4, // Low stock (4 < 10)
                Unit = "Adet",
                Location = "A-Rafı 2",
                PurchasePrice = 40.00m,
                SalePrice = 80.00m,
                MinStockLevel = 10,
                CreatedAt = new DateTime(2026, 5, 2, 11, 30, 0),
                UpdatedAt = new DateTime(2026, 5, 2, 11, 30, 0)
            },
            new Product
            {
                Id = 3,
                Sku = "GID-001",
                Name = "Filtre Kahve 250g",
                CategoryId = 2,
                Quantity = 30,
                Unit = "Paket",
                Location = "B-Rafı 1",
                PurchasePrice = 120.00m,
                SalePrice = 180.00m,
                MinStockLevel = 5,
                CreatedAt = new DateTime(2026, 5, 5, 9, 0, 0),
                UpdatedAt = new DateTime(2026, 5, 5, 9, 0, 0)
            },
            new Product
            {
                Id = 4,
                Sku = "YDK-001",
                Name = "M10 Somun Çelik",
                CategoryId = 3,
                Quantity = 500,
                Unit = "Adet",
                Location = "C-Rafı 3",
                PurchasePrice = 0.50m,
                SalePrice = 1.20m,
                MinStockLevel = 50,
                CreatedAt = new DateTime(2026, 5, 10, 14, 15, 0),
                UpdatedAt = new DateTime(2026, 5, 10, 14, 15, 0)
            },
            new Product
            {
                Id = 5,
                Sku = "OFS-001",
                Name = "A4 Kağıt 500lü",
                CategoryId = 4,
                Quantity = 0, // Out of stock
                Unit = "Kutu",
                Location = "D-Rafı 2",
                PurchasePrice = 90.00m,
                SalePrice = 130.00m,
                MinStockLevel = 8,
                CreatedAt = new DateTime(2026, 5, 12, 16, 0, 0),
                UpdatedAt = new DateTime(2026, 5, 12, 16, 0, 0)
            }
        );
    }
}
