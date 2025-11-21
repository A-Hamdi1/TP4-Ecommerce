using Stripe;
using Stripe.Checkout;
using WebApplication2.Models;
using WebApplication2.Models.Repositories;

namespace WebApplication2.Services
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration configuration;
        private readonly IOrderRepository orderRepository;

        public StripeService(IConfiguration configuration, IOrderRepository orderRepository)
        {
            this.configuration = configuration;
            this.orderRepository = orderRepository;
            // Configure the Stripe API key from configuration
            var secret = configuration["Stripe:SecretKey"];
            if (!string.IsNullOrEmpty(secret))
            {
                StripeConfiguration.ApiKey = secret;
            }
        }

        public async Task<string> CreatePaymentIntentAsync(int orderId, long amountInCents, string currency = "usd")
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = currency,
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", orderId.ToString() }
                }
            };
            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);
            return intent.ClientSecret;
        }

        public async Task<int?> HandleWebhookAsync(string json, string stripeSignatureHeader)
        {
            var webhookSecret = configuration["Stripe:WebhookSecret"];
            Event stripeEvent = null;
            try
            {
                if (!string.IsNullOrEmpty(webhookSecret))
                {
                    stripeEvent = EventUtility.ConstructEvent(json, stripeSignatureHeader, webhookSecret);
                }
                else
                {
                    stripeEvent = EventUtility.ParseEvent(json);
                }
            }
            catch (Exception)
            {
                return null;
            }

            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var pi = stripeEvent.Data.Object as PaymentIntent;
                if (pi != null && pi.Metadata != null && pi.Metadata.TryGetValue("order_id", out var idVal))
                {
                    if (int.TryParse(idVal, out var orderId))
                    {
                        orderRepository.UpdateStatus(orderId, OrderStatus.Paid, null, "Paid via Stripe (webhook)");
                        return orderId;
                    }
                }
            }

            return null;
        }
    }
}
