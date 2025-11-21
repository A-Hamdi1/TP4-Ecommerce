using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        [Required]
        [Display(Name = "Nom")]
        public string CategoryName { get; set; }
        [ValidateNever]
        public ICollection<Product> Products { get; set; }
    }
}
