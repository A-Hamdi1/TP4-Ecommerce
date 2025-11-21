using Microsoft.EntityFrameworkCore;
using WebApplication2.Services;

namespace WebApplication2.Models.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        readonly AppDbContext context;
        private readonly IEmailService _emailService;
        private readonly IInvoiceService _invoiceService;

        public OrderRepository(
            AppDbContext context, 
            IEmailService emailService,
            IInvoiceService invoiceService)
        {
            this.context = context;
            _emailService = emailService;
            _invoiceService = invoiceService;
        }
        public async Task Add(Order o)
        {
            context.Orders.Add(o);
            await context.SaveChangesAsync();
            
            // Envoyer l'email de confirmation standard pour toutes les commandes
            await _emailService.SendOrderConfirmationAsync(o.Email, o.Id, o.CustomerName);

            // Si la commande est payée, générer et envoyer la facture
            if (o.Status == OrderStatus.Paid)
            {
                var pdfInvoice = await _invoiceService.GenerateInvoicePdfAsync(o);
                var emailBody = $@"
                    <h2>Facture de votre commande #{o.Id}</h2>
                    <p>Cher(e) {o.CustomerName},</p>
                    <p>Nous vous remercions pour votre paiement. Vous trouverez ci-joint votre facture au format PDF.</p>
                    <p>Pour suivre votre commande, <a href='http://localhost:5141/Orders/Details/{o.Id}'>cliquez ici</a>.</p>
                    <p>Cordialement,<br>L'équipe E-Commerce</p>
                ";

                await _emailService.SendEmailWithAttachmentAsync(
                    o.Email,
                    $"Facture de votre commande #{o.Id}",
                    emailBody,
                    pdfInvoice,
                    $"facture_{o.Id}.pdf"
                );
            }
        }
        public Order GetById(int id)
        {
            return context.Orders
            .Include(o => o.Items)
            .Include(o => o.History)
            .FirstOrDefault(o => o.Id == id);
        }
        public async Task UpdateStatus(int orderId, OrderStatus newStatus, string changedByUserId = null, string note = null)
        {
            var order = context.Orders.Include(o => o.History).FirstOrDefault(o => o.Id == orderId);
            if (order == null) return;
            order.Status = newStatus;
            var hist = new OrderHistory
            {
                OrderId = orderId,
                Status = newStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = changedByUserId,
                Note = note
            };
            order.History.Add(hist);
            await context.SaveChangesAsync();

            // Envoyer l'email de mise à jour
            if (newStatus != OrderStatus.Pending && newStatus != OrderStatus.Paid)
            {
                await _emailService.SendOrderStatusUpdateAsync(order.Email, order.Id, order.CustomerName, newStatus.ToString());
            }
        }
        public IEnumerable<OrderHistory> GetHistory(int orderId)
        {
            return context.Set<OrderHistory>().Where(h => h.OrderId == orderId).OrderByDescending(h => h.ChangedAt).ToList();
        }
        public IEnumerable<Order> GetAllOrders()
        {
            return context.Orders.Include(o => o.Items).Include(o => o.History).OrderByDescending(o => o.OrderDate).ToList();
        }
        public IEnumerable<Order> GetOrdersByUser(string userId)
        {
            return context.Orders.Include(o => o.Items).Include(o => o.History).Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate).ToList();
        }
    }
}
