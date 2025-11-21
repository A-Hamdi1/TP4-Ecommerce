using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;
using WebApplication2.ViewModels;

namespace WebApplication2.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<ProductAnalytics>> GetTopProductsAsync(int count = 10);
        Task IncrementProductViewCount(int productId);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddDays(-30);
            endDate ??= DateTime.Now;

            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToListAsync();

            var dashboard = new DashboardViewModel
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => (decimal)o.TotalAmount),
                TotalCustomers = await _context.Orders
                    .Select(o => o.UserId)
                    .Distinct()
                    .CountAsync(),
                AverageOrderValue = orders.Any() 
                    ? orders.Average(o => (decimal)o.TotalAmount) 
                    : 0
            };

            // Ventes quotidiennes
            dashboard.DailySales = orders
                .GroupBy(o => o.OrderDate.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(o => (decimal)o.TotalAmount)
                );

            // Ventes mensuelles
            dashboard.MonthlySales = orders
                .GroupBy(o => o.OrderDate.ToString("MMMM yyyy"))
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(o => (decimal)o.TotalAmount)
                );

            // Distribution des statuts de commande
            dashboard.OrderStatusDistribution = orders
                .GroupBy(o => o.Status)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Count()
                );

            // Top produits
            dashboard.TopProducts = await GetTopProductsAsync();

            return dashboard;
        }

        public async Task<List<ProductAnalytics>> GetTopProductsAsync(int count = 10)
        {
            var orderItems = await _context.Orders
                .Include(o => o.Items)
                .SelectMany(o => o.Items)
                .ToListAsync();

            var products = await _context.Products.ToListAsync();

            var analytics = products.Select(p => new ProductAnalytics
            {
                ProductId = p.ProductId,
                ProductName = p.Name,
                ViewCount = p.ViewCount,
                TotalSales = orderItems.Count(oi => oi.ProductName == p.Name),
                TotalRevenue = orderItems
                    .Where(oi => oi.ProductName == p.Name)
                    .Sum(oi => (decimal)oi.Price * oi.Quantity)
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(count)
            .ToList();

            // Calculer le taux de conversion pour chaque produit
            foreach (var product in analytics)
            {
                product.ConversionRate = product.ViewCount > 0 
                    ? (decimal)product.TotalSales / product.ViewCount * 100 
                    : 0;
            }

            return analytics;
        }

        public async Task IncrementProductViewCount(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }
    }
}