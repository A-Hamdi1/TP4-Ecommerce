using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Models;
using WebApplication2.Models.Repositories;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(
            IOrderRepository orderRepository,
            IInvoiceService invoiceService)
        {
            _orderRepository = orderRepository;
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> Download(int orderId)
        {
            var order = _orderRepository.GetById(orderId);
            
            if (order == null)
            {
                return NotFound();
            }

            // Vérifier que l'utilisateur actuel est bien le propriétaire de la commande
            if (order.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            // Vérifier que la commande est payée
            if (order.Status != OrderStatus.Paid)
            {
                TempData["ErrorMessage"] = "La facture n'est disponible que pour les commandes payées.";
                return RedirectToAction("Details", "Orders", new { id = orderId });
            }

            var pdf = await _invoiceService.GenerateInvoicePdfAsync(order);
            return File(pdf, "application/pdf", $"facture_{orderId}.pdf");
        }
    }
}