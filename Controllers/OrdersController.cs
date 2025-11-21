using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models.Repositories;

namespace WebApplication2.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository orderRepository;
        private readonly UserManager<IdentityUser> userManager;

        public OrdersController(IOrderRepository orderRepository, UserManager<IdentityUser> userManager)
        {
            this.orderRepository = orderRepository;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // If the current user is an Admin or Manager, redirect them to the manager UI
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                return RedirectToAction("Index", "ManagerOrder");
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var orders = orderRepository.GetOrdersByUser(user.Id);
            return View(orders);
        }

        public IActionResult Details(int id)
        {
            var order = orderRepository.GetById(id);
            if (order == null) return NotFound();
            // ensure the user owns this order or is admin/manager
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                var uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (uid == null || uid != order.UserId) return Forbid();
            }
            ViewBag.History = orderRepository.GetHistory(id);
            return View(order);
        }
    }
}
