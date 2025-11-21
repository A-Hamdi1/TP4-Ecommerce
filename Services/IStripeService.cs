namespace WebApplication2.Services
{
    public interface IStripeService
    {
        // Create a PaymentIntent and return the client secret to complete payment client-side
        Task<string> CreatePaymentIntentAsync(int orderId, long amountInCents, string currency = "usd");
        // Validate and process webhook payload (returns orderId if processed)
        Task<int?> HandleWebhookAsync(string json, string stripeSignatureHeader);
    }
}
