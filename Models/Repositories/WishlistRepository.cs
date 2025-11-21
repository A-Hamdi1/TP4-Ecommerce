using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Models.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly AppDbContext _context;

        public WishlistRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<WishlistItem> GetUserWishlist(string userId)
        {
            return _context.WishlistItems
                .Include(w => w.Product)
                .ThenInclude(p => p.Category)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedDate)
                .ToList();
        }

        public bool IsInWishlist(string userId, int productId)
        {
            return _context.WishlistItems
                .Any(w => w.UserId == userId && w.ProductId == productId);
        }

        public void AddToWishlist(string userId, int productId)
        {
            if (!IsInWishlist(userId, productId))
            {
                var wishlistItem = new WishlistItem
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedDate = DateTime.UtcNow
                };
                _context.WishlistItems.Add(wishlistItem);
                _context.SaveChanges();
            }
        }

        public void RemoveFromWishlist(string userId, int productId)
        {
            try
            {
                var item = _context.WishlistItems
                    .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);
                
                if (item != null)
                {
                    _context.WishlistItems.Remove(item);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from wishlist: {ex.Message}");
                throw;
            }
        }

        public int GetWishlistCount(string userId)
        {
            return _context.WishlistItems.Count(w => w.UserId == userId);
        }
    }
}