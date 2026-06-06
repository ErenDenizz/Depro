using System.ComponentModel.DataAnnotations;

namespace Proje.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [Display(Name = "Kategori Adı")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    [StringLength(500)]
    public string? Description { get; set; }

    // Navigation property
    public ICollection<Product>? Products { get; set; }
}
