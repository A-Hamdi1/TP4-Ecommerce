using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }
        
        public DateTime AddedDate { get; set; }
    }
}