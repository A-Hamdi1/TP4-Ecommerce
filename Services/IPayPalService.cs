namespace WebApplication2.Services
{
    public interface IPayPalService
    {
        Task<string> CreateOrderAsync(float amount, string currency = "USD");
        Task<bool> CaptureOrderAsync(string orderId);
    }
}

