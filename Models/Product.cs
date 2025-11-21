using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Prix en dinar :")]
        public float Price { get; set; }
        [Required]
        [Display(Name = "Quantité en unité :")]
        public int QteStock { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!; // Initialized as null, EF will manage this
        [Display(Name = "Image :")]
        public string? Image { get; set; } // Made nullable if image is optional
        public int ViewCount { get; set; } = 0; // Nombre de vues du produit
    }
}