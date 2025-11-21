using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Models.Repositories;
using WebApplication2.ViewModels;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "User")]
    public class WishlistController : Controller
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;

        public WishlistController(
            IWishlistRepository wishlistRepository,
            IProductRepository productRepository)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishlist = _wishlistRepository.GetUserWishlist(userId);
            return View(wishlist);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Route("[controller]/[action]/{productId}")]
        public IActionResult ToggleWishlist(int productId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, error = "User ID not found" });
                }

                var product = _productRepository.GetById(productId);
                if (product == null)
                {
                    return Json(new { success = false, error = "Product not found" });
                }

                if (_wishlistRepository.IsInWishlist(userId, productId))
                {
                    _wishlistRepository.RemoveFromWishlist(userId, productId);
                    return Json(new { success = true, isInWishlist = false, count = _wishlistRepository.GetWishlistCount(userId) });
                }
                else
                {
                    _wishlistRepository.AddToWishlist(userId, productId);
                    return Json(new { success = true, isInWishlist = true, count = _wishlistRepository.GetWishlistCount(userId) });
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in ToggleWishlist: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, error = "An error occurred while processing your request" });
            }
        }

        [HttpGet]
        [Route("[controller]/[action]/{productId}")]
        public IActionResult GetWishlistStatus(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isInWishlist = _wishlistRepository.IsInWishlist(userId, productId);
            return Json(new { isInWishlist });
        }
    }
}