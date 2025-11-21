using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace WebApplication2.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(string to, int orderId, string customerName);
        Task SendOrderStatusUpdateAsync(string to, int orderId, string customerName, string newStatus);
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string attachmentName);
        Task SendContactEmailAsync(string to, string subject, string body, string? replyTo = null, string? replyToName = null);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpHost = _configuration["Email:SmtpHost"];
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            _smtpUsername = _configuration["Email:Username"];
            _smtpPassword = _configuration["Email:Password"];
            _fromEmail = _configuration["Email:FromEmail"];
        }

        private async Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null, string? replyTo = null, string? replyToName = null)
        {
            using (var client = new SmtpClient(_smtpHost, _smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                if (!string.IsNullOrEmpty(replyTo))
                {
                    message.ReplyToList.Add(new MailAddress(replyTo, replyToName ?? replyTo));
                }

                if (attachment != null && attachmentName != null)
                {
                    var attachmentStream = new MemoryStream(attachment);
                    message.Attachments.Add(new Attachment(attachmentStream, attachmentName, "application/pdf"));
                }

                await client.SendMailAsync(message);
            }
        }

        public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string attachmentName)
        {
            await SendEmailAsync(to, subject, body, attachment, attachmentName);
        }

        public async Task SendOrderConfirmationAsync(string to, int orderId, string customerName)
        {
            var subject = $"Confirmation de votre commande #{orderId}";
            var body = $@"
                <h2>Merci pour votre commande, {customerName}!</h2>
                <p>Nous avons bien reçu votre commande #{orderId}.</p>
                <p>Nous vous tiendrons informé de l'évolution de votre commande.</p>
                <p>Pour suivre votre commande, <a href='http://localhost:5141/Orders/Details/{orderId}'>cliquez ici</a>.</p>
                <p>Cordialement,<br>L'équipe E-Commerce</p>
            ";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendOrderStatusUpdateAsync(string to, int orderId, string customerName, string newStatus)
        {
            var subject = $"Mise à jour de votre commande #{orderId}";
            var statusMessage = newStatus switch
            {
                "Processing" => "est en cours de préparation",
                "Shipped" => "a été expédiée",
                "Delivered" => "a été livrée",
                "Cancelled" => "a été annulée",
                _ => "a été mise à jour"
            };

            var body = $@"
                <h2>Bonjour {customerName},</h2>
                <p>Votre commande #{orderId} {statusMessage}.</p>
                <p>Pour voir les détails de votre commande, <a href='http://localhost:5141/Orders/Details/{orderId}'>cliquez ici</a>.</p>
                <p>Cordialement,<br>L'équipe E-Commerce</p>
            ";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendContactEmailAsync(string to, string subject, string body, string? replyTo = null, string? replyToName = null)
        {
            await SendEmailAsync(to, subject, body, replyTo: replyTo, replyToName: replyToName);
        }
    }
}