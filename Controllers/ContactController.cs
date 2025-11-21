using Microsoft.AspNetCore.Mvc;
using WebApplication2.Services;
using WebApplication2.ViewModels;

namespace WebApplication2.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public ContactController(IEmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Email à envoyer à l'entreprise
                var adminEmail = _configuration["Email:AdminEmail"] ?? _configuration["Email:FromEmail"];
                var emailSubject = $"Message de contact : {model.Subject}";
                var emailBody = $@"
                    <h2>Nouveau message de contact</h2>
                    <p><strong>Nom :</strong> {model.Name}</p>
                    <p><strong>Email :</strong> {model.Email}</p>
                    <p><strong>Sujet :</strong> {model.Subject}</p>
                    <hr>
                    <p><strong>Message :</strong></p>
                    <p>{model.Message?.Replace("\n", "<br>")}</p>
                ";

                await _emailService.SendContactEmailAsync(adminEmail, emailSubject, emailBody, model.Email ?? "", model.Name ?? "");

                // Email de confirmation au client
                var confirmationSubject = "Confirmation de réception de votre message";
                var confirmationBody = $@"
                    <h2>Bonjour {model.Name},</h2>
                    <p>Nous avons bien reçu votre message concernant : <strong>{model.Subject}</strong></p>
                    <p>Notre équipe vous répondra dans les plus brefs délais.</p>
                    <p>Cordialement,<br>L'équipe NORTHERN</p>
                ";

                if (!string.IsNullOrEmpty(model.Email))
                {
                    await _emailService.SendContactEmailAsync(model.Email, confirmationSubject, confirmationBody);
                }

                TempData["SuccessMessage"] = "Votre message a été envoyé avec succès. Nous vous répondrons bientôt !";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending contact email: {ex.Message}");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'envoi de votre message. Veuillez réessayer plus tard.";
                return View(model);
            }
        }
    }
}

