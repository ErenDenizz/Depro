using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proje.Data;

namespace Proje.Controllers;

public class TransactionsController : Controller
{
    private readonly AppDbContext _context;

    public TransactionsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Transactions
    public async Task<IActionResult> Index(string? searchString, string? typeFilter)
    {
        var query = _context.StockTransactions
            .Include(t => t.Product)
            .ThenInclude(p => p!.Category)
            .AsQueryable();

        // Search filter
        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(t => t.Product!.Name.ToLower().Contains(searchString.ToLower()) || 
                                     t.Product.Sku.ToLower().Contains(searchString.ToLower()) ||
                                     t.Reason!.ToLower().Contains(searchString.ToLower()));
        }

        // Type filter (StockIn/StockOut)
        if (!string.IsNullOrEmpty(typeFilter))
        {
            if (typeFilter == "StockIn")
            {
                query = query.Where(t => t.TransactionType == Models.TransactionType.StockIn);
            }
            else if (typeFilter == "StockOut")
            {
                query = query.Where(t => t.TransactionType == Models.TransactionType.StockOut);
            }
        }

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentType"] = typeFilter;

        return View(transactions);
    }
}
