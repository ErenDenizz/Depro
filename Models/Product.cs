using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proje.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Barkod / SKU kodu zorunludur.")]
    [Display(Name = "Barkod / SKU")]
    [StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [Display(Name = "Ürün Adı")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }

    [Required(ErrorMessage = "Stok miktarı zorunludur.")]
    [Display(Name = "Stok Miktarı")]
    [Range(0, 1000000, ErrorMessage = "Miktar negatif olamaz.")]
    public int Quantity { get; set; } = 0;

    [Required(ErrorMessage = "Birim seçimi zorunludur.")]
    [Display(Name = "Birim")]
    [StringLength(20)]
    public string Unit { get; set; } = "Adet"; // Adet, Kg, Metre, Kutu vb.

    [Display(Name = "Raf Konumu")]
    [StringLength(50)]
    public string? Location { get; set; } // Örn: A-04, Raf-3

    [Display(Name = "Alış Fiyatı")]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 10000000, ErrorMessage = "Fiyat negatif olamaz.")]
    public decimal PurchasePrice { get; set; } = 0.00m;

    [Display(Name = "Satış Fiyatı")]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 10000000, ErrorMessage = "Fiyat negatif olamaz.")]
    public decimal SalePrice { get; set; } = 0.00m;

    [Required(ErrorMessage = "Kritik stok seviyesi zorunludur.")]
    [Display(Name = "Kritik Stok Sınırı")]
    [Range(0, 100000, ErrorMessage = "Kritik sınır negatif olamaz.")]
    public int MinStockLevel { get; set; } = 5;

    [Display(Name = "Kayıt Tarihi")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "Güncelleme Tarihi")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
