using System;
using System.Collections.Generic;

namespace WebApplication2.ViewModels
{
    public class DashboardViewModel
    {
        // Statistiques générales
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }

        // Statistiques de vente par période
        public Dictionary<DateTime, decimal> DailySales { get; set; }
        public Dictionary<string, decimal> MonthlySales { get; set; }

        // Top produits
        public List<ProductAnalytics> TopProducts { get; set; }
        
        // Statistiques des commandes
        public Dictionary<string, int> OrderStatusDistribution { get; set; }

        public DashboardViewModel()
        {
            DailySales = new Dictionary<DateTime, decimal>();
            MonthlySales = new Dictionary<string, decimal>();
            TopProducts = new List<ProductAnalytics>();
            OrderStatusDistribution = new Dictionary<string, int>();
        }
    }

    public class ProductAnalytics
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ViewCount { get; set; }
        public decimal ConversionRate { get; set; }  // ViewCount/Sales ratio
    }
}