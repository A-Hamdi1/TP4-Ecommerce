using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;
using WebApplication2.Models.Repositories;
using WebApplication2.ViewModels;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly IProductRepository _productRepository;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, IProductRepository productRepository)
        {
            _logger = logger;
            _context = context;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            // Récupérer les top 4 produits les plus vendus
            var topProducts = await GetTopSellingProducts(4);
            viewModel.TopProducts = topProducts;

            // Données statiques pour la promotion
            viewModel.Promotion = new Promotion
            {
                Title = "Promotion Spéciale - Jusqu'à 50% de réduction !",
                Description = "Profitez de nos offres exceptionnelles sur une sélection de produits premium. Ne manquez pas cette opportunité unique !",
                ButtonText = "Voir les produits",
                ButtonLink = "/Product"
            };

            // Avis clients statiques
            viewModel.Reviews = new List<CustomerReview>
            {
                new CustomerReview
                {
                    CustomerName = "Sophie Martin",
                    Comment = "Excellent service et produits de qualité. Livraison rapide et emballage soigné. Je recommande vivement !",
                    Rating = 4,
                    Avatar = "👩"
                },
                new CustomerReview
                {
                    CustomerName = "Thomas Dubois",
                    Comment = "Très satisfait de mon achat. Le produit correspond parfaitement à la description. Service client réactif et professionnel.",
                    Rating = 4,
                    Avatar = "👨"
                },
                new CustomerReview
                {
                    CustomerName = "Marie Leclerc",
                    Comment = "Une expérience d'achat agréable du début à la fin. Produits de qualité supérieure et prix compétitifs. Je reviendrai !",
                    Rating = 4,
                    Avatar = "👩‍💼"
                }
            };

            return View(viewModel);
        }

        private async Task<List<Product>> GetTopSellingProducts(int count)
        {
            // Récupérer les produits les plus vendus basés sur OrderItems
            var topProductNames = await _context.OrderItems
                .GroupBy(oi => oi.ProductName)
                .Select(g => new { ProductName = g.Key, TotalQuantity = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(count)
                .Select(x => x.ProductName)
                .ToListAsync();

            // Récupérer les produits correspondants avec leurs catégories
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => topProductNames.Contains(p.Name))
                .ToListAsync();

            // Trier selon l'ordre des topProductNames
            var orderedProducts = topProductNames
                .Select(name => products.FirstOrDefault(p => p.Name == name))
                .Where(p => p != null)
                .Cast<Product>()
                .ToList();

            // Si on n'a pas assez de produits vendus, compléter avec les produits les plus récents
            if (orderedProducts.Count < count)
            {
                var additionalProducts = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => !topProductNames.Contains(p.Name))
                    .OrderByDescending(p => p.ProductId)
                    .Take(count - orderedProducts.Count)
                    .ToListAsync();

                orderedProducts.AddRange(additionalProducts);
            }

            return orderedProducts.Take(count).ToList();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
