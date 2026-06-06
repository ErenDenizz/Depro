using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proje.Data;
using Proje.Models;

namespace Proje.Controllers;

public class ProductsController : Controller
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Products
    public async Task<IActionResult> Index(string? searchString, int? categoryId, string? statusFilter)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        // Search filter (name, SKU, location)
        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(p => p.Name.ToLower().Contains(searchString.ToLower()) || 
                                     p.Sku.ToLower().Contains(searchString.ToLower()) ||
                                     (p.Location != null && p.Location.ToLower().Contains(searchString.ToLower())));
        }

        // Category filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Status filter (Low Stock, Out of Stock, In Stock)
        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "LowStock")
            {
                query = query.Where(p => p.Quantity <= p.MinStockLevel && p.Quantity > 0);
            }
            else if (statusFilter == "OutOfStock")
            {
                query = query.Where(p => p.Quantity == 0);
            }
            else if (statusFilter == "NormalStock")
            {
                query = query.Where(p => p.Quantity > p.MinStockLevel);
            }
        }

        var products = await query.ToListAsync();

        // ViewBags for dropdowns and current filters
        ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentCategory"] = categoryId;
        ViewData["CurrentStatus"] = statusFilter;

        return View(products);
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (product == null)
        {
            return NotFound();
        }

        // Fetch recent transactions for this product
        var transactions = await _context.StockTransactions
            .Where(t => t.ProductId == id)
            .OrderByDescending(t => t.TransactionDate)
            .Take(10)
            .ToListAsync();

        ViewBag.Transactions = transactions;

        return View(product);
    }

    // GET: Products/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
        return View();
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Sku,Name,CategoryId,Quantity,Unit,Location,PurchasePrice,SalePrice,MinStockLevel")] Product product)
    {
        // Check SKU uniqueness
        if (await _context.Products.AnyAsync(p => p.Sku == product.Sku))
        {
            ModelState.AddModelError("Sku", "Bu barkod / SKU kodu zaten kullanımda.");
        }

        if (ModelState.IsValid)
        {
            product.CreatedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;

            _context.Add(product);
            await _context.SaveChangesAsync();

            // If initial quantity is greater than 0, create a transaction entry
            if (product.Quantity > 0)
            {
                var transaction = new StockTransaction
                {
                    ProductId = product.Id,
                    TransactionType = TransactionType.StockIn,
                    Quantity = product.Quantity,
                    Reason = "İlk Sayım Girişi",
                    TransactionDate = DateTime.Now,
                    PerformedBy = "Sistem"
                };
                _context.Add(transaction);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Ürün başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    // GET: Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    // POST: Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Sku,Name,CategoryId,Quantity,Unit,Location,PurchasePrice,SalePrice,MinStockLevel,CreatedAt")] Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        // Check SKU uniqueness excluding itself
        if (await _context.Products.AnyAsync(p => p.Sku == product.Sku && p.Id != id))
        {
            ModelState.AddModelError("Sku", "Bu barkod / SKU kodu başka bir ürün tarafından kullanılıyor.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                product.UpdatedAt = DateTime.Now;
                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    // GET: Products/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            // Remove associated transactions first to prevent foreign key errors
            var transactions = await _context.StockTransactions.Where(t => t.ProductId == id).ToListAsync();
            _context.StockTransactions.RemoveRange(transactions);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Ürün ve bağlı tüm stok geçmişi silindi.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Products/StockMove/5
    public async Task<IActionResult> StockMove(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        var transaction = new StockTransaction
        {
            ProductId = product.Id,
            Product = product
        };

        return View(transaction);
    }

    // POST: Products/StockMove/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StockMove(int id, [Bind("ProductId,TransactionType,Quantity,Reason,PerformedBy")] StockTransaction transaction)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        transaction.ProductId = id;
        transaction.TransactionDate = DateTime.Now;

        // Perform validation
        if (!transaction.Quantity.HasValue || transaction.Quantity.Value <= 0)
        {
            ModelState.AddModelError("Quantity", "Miktar 0'dan büyük olmalıdır.");
        }

        if (transaction.TransactionType == TransactionType.StockOut && transaction.Quantity.HasValue && product.Quantity < transaction.Quantity.Value)
        {
            ModelState.AddModelError("Quantity", $"Yetersiz stok. Çıkış yapılmak istenen miktar ({transaction.Quantity.Value}), mevcut stoktan ({product.Quantity}) fazladır.");
        }

        if (ModelState.IsValid)
        {
            // Update product stock quantity
            if (transaction.TransactionType == TransactionType.StockIn)
            {
                product.Quantity += transaction.Quantity!.Value;
                TempData["SuccessMessage"] = $"{product.Name} ürünü için {transaction.Quantity.Value} {product.Unit} stok girişi yapıldı.";
            }
            else
            {
                product.Quantity -= transaction.Quantity!.Value;
                TempData["SuccessMessage"] = $"{product.Name} ürünü için {transaction.Quantity.Value} {product.Unit} stok çıkışı yapıldı.";
            }

            product.UpdatedAt = DateTime.Now;
            _context.Update(product);

            // Add transaction log
            _context.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        transaction.Product = product;
        return View(transaction);
    }

    // GET: Products/ExportCsv
    public async Task<IActionResult> ExportCsv(string? searchString, int? categoryId, string? statusFilter)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(p => p.Name.ToLower().Contains(searchString.ToLower()) || 
                                     p.Sku.ToLower().Contains(searchString.ToLower()) ||
                                     (p.Location != null && p.Location.ToLower().Contains(searchString.ToLower())));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "LowStock")
            {
                query = query.Where(p => p.Quantity <= p.MinStockLevel && p.Quantity > 0);
            }
            else if (statusFilter == "OutOfStock")
            {
                query = query.Where(p => p.Quantity == 0);
            }
            else if (statusFilter == "NormalStock")
            {
                query = query.Where(p => p.Quantity > p.MinStockLevel);
            }
        }

        var products = await query.ToListAsync();

        var csvBuilder = new System.Text.StringBuilder();
        // CSV Header - using semicolon separator for direct Excel compatibility in Europe/Turkey
        csvBuilder.AppendLine("Barkod/SKU;Ürün Adı;Kategori;Mevcut Stok;Birim;Raf Konumu;Alış Fiyatı;Satış Fiyatı;Kritik Seviye;Kayıt Tarihi;Son Güncelleme");

        foreach (var p in products)
        {
            csvBuilder.AppendLine($"{p.Sku};{p.Name};{p.Category?.Name ?? "-"};{p.Quantity};{p.Unit};{p.Location ?? "-"};{p.PurchasePrice:F2};{p.SalePrice:F2};{p.MinStockLevel};{p.CreatedAt:dd.MM.yyyy HH:mm};{p.UpdatedAt:dd.MM.yyyy HH:mm}");
        }

        // Add UTF-8 BOM so Excel opens Turkish characters correctly
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var fileContent = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
        var finalContent = bom.Concat(fileContent).ToArray();

        return File(finalContent, "text/csv", $"depo_stok_raporu_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}
