using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proje.Models;

public enum TransactionType
{
    [Display(Name = "Stok Girişi")]
    StockIn,
    
    [Display(Name = "Stok Çıkışı")]
    StockOut
}

public class StockTransaction
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Ürün")]
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [Required]
    [Display(Name = "İşlem Türü")]
    public TransactionType TransactionType { get; set; }

    [Required(ErrorMessage = "Miktar alanı doldurulmalıdır.")]
    [Display(Name = "Miktar")]
    [Range(1, 1000000, ErrorMessage = "Miktar en az 1 olmalıdır.")]
    public int? Quantity { get; set; }

    [Display(Name = "Açıklama / Neden")]
    [StringLength(250)]
    public string? Reason { get; set; } // Örn: Satış, Yeni Alım, Fire, İade, Sayım Düzeltmesi

    [Display(Name = "İşlem Tarihi")]
    public DateTime TransactionDate { get; set; } = DateTime.Now;

    [Display(Name = "İşlemi Yapan")]
    [StringLength(100)]
    public string PerformedBy { get; set; } = "Yönetici";
}
