using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using WebApplication2.Models.Help;
using WebApplication2.Models.Repositories;
using WebApplication2.ViewModels;

namespace WebApplication2.Controllers
{
    public class PanierController : Controller
    {
        readonly IProductRepository productRepository;
        readonly IOrderRepository orderRepository;
        private readonly UserManager<IdentityUser> userManager;
        public PanierController(IProductRepository productRepository, IOrderRepository orderRepository,
UserManager<IdentityUser> userManager)
        {
            this.productRepository = productRepository;
            this.orderRepository = orderRepository;
            this.userManager = userManager;
        }
        public ActionResult Index()
        {
            ViewBag.Liste = ListeCart.Instance.Items;
            ViewBag.total = ListeCart.Instance.GetSubTotal();
            return View();
        }
        [Authorize(Roles = "User,Admin,Manager")]
        public ActionResult AddProduct(int id)
        {
            Product pp = productRepository.GetById(id);
            ListeCart.Instance.AddItem(pp);
            ViewBag.Liste = ListeCart.Instance.Items;
            ViewBag.total = ListeCart.Instance.GetSubTotal();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "User,Admin,Manager")]
        public ActionResult PlusProduct(int id)
        {
            Product pp = productRepository.GetById(id);
            ListeCart.Instance.AddItem(pp);
            Item trouve = null;
            foreach (Item a in ListeCart.Instance.Items)
            {
                if (a.Prod.ProductId == pp.ProductId)
                    trouve = a;
            }
            var results = new
            {
                ct = 1,
                Total = ListeCart.Instance.GetSubTotal(),
                Quatite = trouve.quantite,
                TotalRow = trouve.TotalPrice
            };
            return Json(results);
        }
        [HttpPost]
        [Authorize(Roles = "User,Admin,Manager")]
        public ActionResult MinusProduct(int id)
        {
            Product pp = productRepository.GetById(id);
            ListeCart.Instance.SetLessOneItem(pp);
            Item trouve = null;
            foreach (Item a in ListeCart.Instance.Items)
            {
                if (a.Prod.ProductId == pp.ProductId)
                    trouve = a;
            }
            if (trouve != null)
            {
                var results = new
                {
                    Total = ListeCart.Instance.GetSubTotal(),
                    Quatite = trouve.quantite,
                    TotalRow = trouve.TotalPrice,
                    ct = 1
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    ct = 0
                };
                return Json(results);
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin,Manager")]
        public ActionResult RemoveProduct(int id)
        {
            Product pp = productRepository.GetById(id);
            ListeCart.Instance.RemoveItem(pp);
            var results = new
            {
                Total = ListeCart.Instance.GetSubTotal(),
            };
            return Json(results);
        }
        [Authorize]
        // GET: /Order/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }
            var cartItems = ListeCart.Instance.Items.ToList();
            var totalAmount = ListeCart.Instance.GetSubTotal();
            var viewModel = new OrderViewModel
            {
                CartItems = cartItems.Select(item => new CartItemViewModel
                {
                    ProductName = item.Prod.Name,
                    Quantity = item.quantite,
                    Price = item.Prod.Price
                }).ToList(),
                TotalAmount = totalAmount
            };
            return View(viewModel);
        }
        // POST : /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Checkout(OrderViewModel model)
        {
            // Recharger les articles du panier pour la vue en cas d'erreur
            var cartItems = ListeCart.Instance.Items.ToList();
            model.CartItems = cartItems.Select(item => new CartItemViewModel
            {
                ProductName = item.Prod.Name,
                Quantity = item.quantite,
                Price = item.Prod.Price
            }).ToList();
            model.TotalAmount = ListeCart.Instance.GetSubTotal();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur non authentifié.";
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }

            // Recharger les articles du panier depuis la source (ListeCart)
            if (cartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
                return RedirectToAction("Index");
            }

            // Stocker les données de commande en session pour les utiliser après paiement
            var orderData = new
            {
                CustomerName = user.UserName,
                Email = user.Email,
                Address = model.Address,
                PaymentMethod = model.PaymentMethod,
                TotalAmount = model.TotalAmount,
                CartItems = model.CartItems,
                UserId = user.Id
            };

            HttpContext.Session.SetString("PendingOrder", System.Text.Json.JsonSerializer.Serialize(orderData));

            // Gérer selon la méthode de paiement
            if (model.PaymentMethod == "CreditCard")
            {
                // Rediriger vers Stripe (la commande sera créée après paiement réussi)
                return RedirectToAction("CreatePayment", "Stripe");
            }
            else if (model.PaymentMethod == "Paypal")
            {
                // Rediriger vers PayPal (la commande sera créée après paiement réussi)
                return RedirectToAction("CreatePayment", "PayPal");
            }
            else if (model.PaymentMethod == "CashOnDelivery")
            {
                try
                {
                    // Paiement à domicile - créer la commande directement
                    if (model.CartItems == null || !model.CartItems.Any())
                    {
                        TempData["ErrorMessage"] = "Votre panier est vide.";
                        return RedirectToAction("Index");
                    }

                    var order = new Order
                    {
                        CustomerName = user.UserName ?? "Client",
                        Email = user.Email ?? "",
                        Address = model.Address ?? "",
                        PaymentMethod = "À domicile",
                        TotalAmount = model.TotalAmount,
                        OrderDate = DateTime.Now,
                        UserId = user.Id,
                        Status = OrderStatus.Pending,
                        Items = new List<OrderItem>()
                    };
                    
                    // Ajouter les articles de la commande
                    foreach (var item in model.CartItems)
                    {
                        if (item != null && !string.IsNullOrEmpty(item.ProductName))
                        {
                            order.Items.Add(new OrderItem
                            {
                                ProductName = item.ProductName,
                                Quantity = item.Quantity,
                                Price = item.Price
                            });
                        }
                    }
                    
                    // Vérifier qu'il y a au moins un article
                    if (!order.Items.Any())
                    {
                        TempData["ErrorMessage"] = "Aucun article valide dans votre panier.";
                        return RedirectToAction("Index");
                    }
                    
                    await orderRepository.Add(order);
                    
                    // Mettre à jour le stock pour chaque produit
                    foreach (var cartItem in cartItems)
                    {
                        if (!productRepository.UpdateStock(cartItem.Prod.ProductId, cartItem.quantite))
                        {
                            // Si la mise à jour du stock échoue, on annule la commande
                            TempData["ErrorMessage"] = $"Stock insuffisant pour le produit {cartItem.Prod.Name}";
                            return RedirectToAction("Index");
                        }
                    }
                    
                    ListeCart.Instance.Items.Clear();
                    HttpContext.Session.Remove("PendingOrder");
                    return RedirectToAction("Confirmation", new { orderId = order.Id });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating order for CashOnDelivery: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    TempData["ErrorMessage"] = "Une erreur est survenue lors de la création de la commande. Veuillez réessayer.";
                    return View(model);
                }
            }
            
            TempData["ErrorMessage"] = "Méthode de paiement invalide.";
            return View(model);
        }

        // Méthode pour créer la commande après paiement réussi (appelée par Stripe/PayPal)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrderAfterPayment(string paymentMethod)
        {
            var orderDataJson = HttpContext.Session.GetString("PendingOrder");
            if (string.IsNullOrEmpty(orderDataJson))
            {
                return BadRequest("Données de commande introuvables.");
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Désérialiser les données
            using var doc = System.Text.Json.JsonDocument.Parse(orderDataJson);
            var root = doc.RootElement;

            var order = new Order
            {
                CustomerName = root.GetProperty("CustomerName").GetString(),
                Email = root.GetProperty("Email").GetString(),
                Address = root.GetProperty("Address").GetString(),
                PaymentMethod = paymentMethod ?? root.GetProperty("PaymentMethod").GetString(),
                TotalAmount = (float)root.GetProperty("TotalAmount").GetDouble(),
                OrderDate = DateTime.Now,
                UserId = root.GetProperty("UserId").GetString(),
                Status = OrderStatus.Paid,
                Items = root.GetProperty("CartItems").EnumerateArray().Select(item => new OrderItem
                {
                    ProductName = item.GetProperty("ProductName").GetString(),
                    Quantity = item.GetProperty("Quantity").GetInt32(),
                    Price = (float)item.GetProperty("Price").GetDouble()
                }).ToList()
            };

            orderRepository.Add(order);
            ListeCart.Instance.Items.Clear();
            HttpContext.Session.Remove("PendingOrder");

            return Json(new { orderId = order.Id });
        }

        // GET: /Order/Confirmation
        public IActionResult Confirmation(int orderId)
        {
            var order = orderRepository.GetById(orderId);
            return View(order);
        }
    }

}
