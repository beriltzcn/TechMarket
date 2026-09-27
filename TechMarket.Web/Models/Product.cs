using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMarket.Web.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Product name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(60)]
        [Display(Name = "Brand")]
        public string Brand { get; set; } = string.Empty;

        [StringLength(60)]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Range(0, 1000000)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Range(0, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Discount price")]
        public decimal? DiscountPrice { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        // This is what goes into the database: a web path, not the file itself.
        [StringLength(300)]
        [Display(Name = "Image path")]
        public string? ImageUrl { get; set; }

        // [NotMapped] tells EF Core: "this is NOT a database column, ignore it".
        [NotMapped]
        [Display(Name = "Product image")]
        public IFormFile? ImageFile { get; set; }

        [NotMapped]
        public bool HasDiscount => DiscountPrice is > 0 && DiscountPrice < Price;

        [NotMapped]
        public decimal EffectivePrice => HasDiscount ? DiscountPrice!.Value : Price;

        [NotMapped]
        public int DiscountPercent => HasDiscount
            ? (int)Math.Round((Price - DiscountPrice!.Value) / Price * 100, MidpointRounding.AwayFromZero)
            : 0;
    }

}