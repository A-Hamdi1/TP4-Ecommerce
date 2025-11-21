namespace WebApplication2.Models.Repositories
{
    public interface IWishlistRepository
    {
        IEnumerable<WishlistItem> GetUserWishlist(string userId);
        bool IsInWishlist(string userId, int productId);
        void AddToWishlist(string userId, int productId);
        void RemoveFromWishlist(string userId, int productId);
        int GetWishlistCount(string userId);
    }
}