using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class OrderHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        // user who made the change (optional)
        public string ChangedByUserId { get; set; }
        public IdentityUser ChangedBy { get; set; }
        public string Note { get; set; }
    }
}
