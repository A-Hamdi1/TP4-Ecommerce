using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Services;
using WebApplication2.Models.Repositories;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class StripeController : Controller
    {
    private readonly IStripeService stripeService;
    private readonly IOrderRepository orderRepository;
    private readonly IConfiguration configuration;
    private readonly UserManager<IdentityUser> userManager;
    private readonly IProductRepository productRepository;

        public StripeController(
            IStripeService stripeService, 
            IOrderRepository orderRepository, 
            IConfiguration configuration, 
            UserManager<IdentityUser> userManager,
            IProductRepository productRepository)
        {
            this.stripeService = stripeService;
            this.orderRepository = orderRepository;
            this.configuration = configuration;
            this.userManager = userManager;
            this.productRepository = productRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreatePayment()
        {
            var orderDataJson = HttpContext.Session.GetString("PendingOrder");
            if (string.IsNullOrEmpty(orderDataJson))
            {
                TempData["ErrorMessage"] = "Session expirée. Veuillez recommencer.";
                return RedirectToAction("Index", "Panier");
            }

            // Désérialiser les données pour obtenir le montant
            using var doc = System.Text.Json.JsonDocument.Parse(orderDataJson);
            var root = doc.RootElement;
            var totalAmount = (float)root.GetProperty("TotalAmount").GetDouble();

            // Créer un PaymentIntent temporaire (sans orderId pour l'instant)
            var amountInCents = (long)((decimal)totalAmount * 100m);
            var clientSecret = await stripeService.CreatePaymentIntentAsync(0, amountInCents); // orderId = 0 temporairement
            
            ViewBag.ClientSecret = clientSecret;
            ViewBag.PublishableKey = configuration["Stripe:PublishableKey"];
            return View("PaymentPage");
        }

        [HttpPost]
        [Authorize]
        [IgnoreAntiforgeryToken] // On ignore car on gère manuellement via fetch
        public async Task<IActionResult> PaymentSuccess()
        {
            var orderDataJson = HttpContext.Session.GetString("PendingOrder");
            if (string.IsNullOrEmpty(orderDataJson))
            {
                TempData["ErrorMessage"] = "Session expirée.";
                return RedirectToAction("Index", "Panier");
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Désérialiser les données
            using var doc = System.Text.Json.JsonDocument.Parse(orderDataJson);
            var root = doc.RootElement;

            // Créer la commande maintenant que le paiement est réussi
            var order = new Order
            {
                CustomerName = root.GetProperty("CustomerName").GetString() ?? "",
                Email = root.GetProperty("Email").GetString() ?? "",
                Address = root.GetProperty("Address").GetString() ?? "",
                PaymentMethod = "Carte de crédit (Stripe)",
                TotalAmount = (float)root.GetProperty("TotalAmount").GetDouble(),
                OrderDate = DateTime.Now,
                UserId = root.GetProperty("UserId").GetString() ?? "",
                Status = OrderStatus.Paid,
                Items = root.GetProperty("CartItems").EnumerateArray().Select(item => new OrderItem
                {
                    ProductName = item.GetProperty("ProductName").GetString() ?? "",
                    Quantity = item.GetProperty("Quantity").GetInt32(),
                    Price = (float)item.GetProperty("Price").GetDouble()
                }).ToList()
            };

            await orderRepository.Add(order);
            
            // Mettre à jour le stock pour chaque produit
            var cartItems = Models.Help.ListeCart.Instance.Items.ToList();
            foreach (var item in cartItems)
            {
                if (!productRepository.UpdateStock(item.Prod.ProductId, item.quantite))
                {
                    // Si la mise à jour du stock échoue, on annule la commande
                    TempData["ErrorMessage"] = $"Stock insuffisant pour le produit {item.Prod.Name}";
                    return RedirectToAction("Index", "Panier");
                }
            }
            foreach (var item in cartItems)
            {
                Models.Help.ListeCart.Instance.RemoveItem(item.Prod);
            }
            HttpContext.Session.Remove("PendingOrder");

            return Json(new { orderId = order.Id });
        }

        [HttpGet]
        [Authorize]
        public IActionResult PaymentSuccessView(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View("PaymentSuccess");
        }

        [HttpGet]
        public IActionResult PaymentCancel()
        {
            var orderDataJson = HttpContext.Session.GetString("PendingOrder");
            if (!string.IsNullOrEmpty(orderDataJson))
            {
                using var doc = System.Text.Json.JsonDocument.Parse(orderDataJson);
                var root = doc.RootElement;
                // Ne pas créer de commande, juste afficher l'annulation
            }
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            var sig = Request.Headers["Stripe-Signature"].ToString();
            var processedOrderId = await stripeService.HandleWebhookAsync(json, sig);
            if (processedOrderId.HasValue)
            {
                return Ok();
            }
            return BadRequest();
        }
    }
}
