using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : Controller
    {
        private readonly IAnalyticsService _analyticsService;

        public DashboardController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null)
        {
            var dashboardData = await _analyticsService.GetDashboardDataAsync(startDate, endDate);
            return View(dashboardData);
        }

        [HttpGet]
        public async Task<IActionResult> TopProducts()
        {
            var products = await _analyticsService.GetTopProductsAsync();
            return Json(products);
        }

        [HttpGet]
        public async Task<JsonResult> GetSalesData(DateTime? startDate = null, DateTime? endDate = null)
        {
            var dashboard = await _analyticsService.GetDashboardDataAsync(startDate, endDate);
            return Json(new
            {
                dailySales = dashboard.DailySales,
                monthlySales = dashboard.MonthlySales,
                orderStatus = dashboard.OrderStatusDistribution
            });
        }
    }
}