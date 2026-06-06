using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proje.Data;
using Proje.Models;

namespace Proje.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(AppDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // Totals
        var totalProducts = await _context.Products.CountAsync();
        var totalCategories = await _context.Categories.CountAsync();
        
        var products = await _context.Products.ToListAsync();
        var totalStockQuantity = products.Sum(p => p.Quantity);
        
        // Low Stock (Quantity <= MinStockLevel and Quantity > 0)
        var lowStockCount = products.Count(p => p.Quantity <= p.MinStockLevel && p.Quantity > 0);
        
        // Out of Stock (Quantity == 0)
        var outOfStockCount = products.Count(p => p.Quantity == 0);

        // Valuation
        var totalPurchaseValue = products.Sum(p => p.Quantity * p.PurchasePrice);
        var totalSaleValue = products.Sum(p => p.Quantity * p.SalePrice);
        var potentialProfit = totalSaleValue - totalPurchaseValue;

        // Recent Movements (Last 5 transactions)
        var recentTransactions = await _context.StockTransactions
            .Include(t => t.Product)
            .OrderByDescending(t => t.TransactionDate)
            .Take(5)
            .ToListAsync();

        // Low stock products list (top 5 for home page alerts)
        var criticalProducts = products
            .Where(p => p.Quantity <= p.MinStockLevel)
            .OrderBy(p => p.Quantity)
            .Take(5)
            .ToList();

        // Stats by category
        var categoryStats = await _context.Categories
            .Select(c => new CategoryStatViewModel
            {
                CategoryName = c.Name,
                ProductCount = c.Products != null ? c.Products.Count : 0,
                TotalStock = c.Products != null ? c.Products.Sum(p => p.Quantity) : 0
            })
            .ToListAsync();

        // Pass to View via ViewBag
        ViewBag.TotalProducts = totalProducts;
        ViewBag.TotalCategories = totalCategories;
        ViewBag.TotalStockQuantity = totalStockQuantity;
        ViewBag.LowStockCount = lowStockCount;
        ViewBag.OutOfStockCount = outOfStockCount;
        ViewBag.TotalPurchaseValue = totalPurchaseValue;
        ViewBag.TotalSaleValue = totalSaleValue;
        ViewBag.PotentialProfit = potentialProfit;
        ViewBag.RecentTransactions = recentTransactions;
        ViewBag.CriticalProducts = criticalProducts;
        ViewBag.CategoryStats = categoryStats;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

// ViewModel for Category Dashboard Stats
public class CategoryStatViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalStock { get; set; }
}
