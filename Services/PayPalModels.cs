using System.Text.Json.Serialization;

namespace WebApplication2.Services
{
    public class PayPalOrderRequest
    {
        [JsonPropertyName("intent")]
        public string Intent { get; set; } = "CAPTURE";

        [JsonPropertyName("purchase_units")]
        public List<PayPalPurchaseUnit> PurchaseUnits { get; set; } = new();
    }

    public class PayPalPurchaseUnit
    {
        [JsonPropertyName("amount")]
        public PayPalAmount Amount { get; set; } = new();
    }

    public class PayPalAmount
    {
        [JsonPropertyName("currency_code")]
        public string CurrencyCode { get; set; } = "USD";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";
    }
}

