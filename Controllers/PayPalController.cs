using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using WebApplication2.Models.Repositories;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [Authorize]
    public class PayPalController : Controller
    {
        private readonly IOrderRepository orderRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IPayPalService payPalService;
        private readonly IConfiguration configuration;
        private readonly IProductRepository productRepository;

        public PayPalController(
            IOrderRepository orderRepository, 
            UserManager<IdentityUser> userManager,
            IPayPalService payPalService,
            IConfiguration configuration,
            IProductRepository productRepository)
        {
            this.orderRepository = orderRepository;
            this.userManager = userManager;
            this.payPalService = payPalService;
            this.configuration = configuration;
            this.productRepository = productRepository;
        }

        [HttpGet]
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

            // Passer le montant à la vue pour que le SDK PayPal crée l'ordre côté client
            ViewBag.TotalAmount = totalAmount;
            ViewBag.ClientId = configuration["PayPal:ClientId"];
            ViewBag.UseSandbox = configuration.GetValue<bool>("PayPal:UseSandbox", true);
            
            return View("PaymentPage");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentRequest request)
        {
            if (string.IsNullOrEmpty(request.OrderId))
            {
                return BadRequest(new { error = "OrderId is required" });
            }

            var orderDataJson = HttpContext.Session.GetString("PendingOrder");
            if (string.IsNullOrEmpty(orderDataJson))
            {
                return BadRequest(new { error = "Session expired" });
            }

            try
            {
                // Capturer le paiement PayPal
                var success = await payPalService.CaptureOrderAsync(request.OrderId);
                
                if (!success)
                {
                    return BadRequest(new { error = "Payment capture failed - order status is not COMPLETED" });
                }

                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Désérialiser les données
                using var doc = System.Text.Json.JsonDocument.Parse(orderDataJson);
                var root = doc.RootElement;

                // Créer la commande maintenant que le paiement PayPal est réussi
                var order = new Order
                {
                    CustomerName = root.GetProperty("CustomerName").GetString() ?? "",
                    Email = root.GetProperty("Email").GetString() ?? "",
                    Address = root.GetProperty("Address").GetString() ?? "",
                    PaymentMethod = "PayPal",
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
                        return BadRequest(new { error = $"Stock insuffisant pour le produit {item.Prod.Name}" });
                    }
                }
                foreach (var item in cartItems)
                {
                    Models.Help.ListeCart.Instance.RemoveItem(item.Prod);
                }
                HttpContext.Session.Remove("PendingOrder");

                return Json(new { orderId = order.Id, success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult PaymentSuccess(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpGet]
        public IActionResult PaymentCancel()
        {
            return View();
        }
    }

    public class CapturePaymentRequest
    {
        public string OrderId { get; set; } = "";
    }
}
