using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Models.Repositories;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ManagerOrderController : Controller
    {
        private readonly IOrderRepository orderRepository;

        public ManagerOrderController(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        public IActionResult Index()
        {
            var orders = orderRepository.GetAllOrders();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await orderRepository.UpdateStatus(id, status, userId, $"Updated by manager {userId}");
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var order = orderRepository.GetById(id);
            if (order == null) return NotFound();
            var history = orderRepository.GetHistory(id);
            ViewBag.History = history;
            return View(order);
        }
    }
}
