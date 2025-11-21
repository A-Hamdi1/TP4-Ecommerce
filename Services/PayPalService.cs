using System.Text;
using System.Text.Json;

namespace WebApplication2.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
        private string? accessToken;

        public PayPalService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(accessToken))
                return accessToken;

            var clientId = configuration["PayPal:ClientId"];
            var clientSecret = configuration["PayPal:ClientSecret"];
            var isSandbox = configuration.GetValue<bool>("PayPal:UseSandbox", true);
            var baseUrl = isSandbox 
                ? "https://api.sandbox.paypal.com" 
                : "https://api.paypal.com";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException("PayPal credentials not configured. Please set PayPal:ClientId and PayPal:ClientSecret in appsettings.json");
            }

            var httpClient = httpClientFactory.CreateClient();
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token");
            request.Headers.Add("Authorization", $"Basic {auth}");
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to get PayPal access token: {response.StatusCode} - {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);
            accessToken = tokenResponse.GetProperty("access_token").GetString();
            
            return accessToken ?? throw new InvalidOperationException("Failed to get PayPal access token");
        }

        public async Task<string> CreateOrderAsync(float amount, string currency = "USD")
        {
            var token = await GetAccessTokenAsync();
            var isSandbox = configuration.GetValue<bool>("PayPal:UseSandbox", true);
            var baseUrl = isSandbox 
                ? "https://api.sandbox.paypal.com" 
                : "https://api.paypal.com";

            var httpClient = httpClientFactory.CreateClient();
            
            // Format du montant : utiliser le point comme séparateur décimal et pas de séparateur de milliers
            var amountValue = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            
            // Créer la requête PayPal avec le bon format
            var orderRequest = new PayPalOrderRequest
            {
                Intent = "CAPTURE",
                PurchaseUnits = new List<PayPalPurchaseUnit>
                {
                    new PayPalPurchaseUnit
                    {
                        Amount = new PayPalAmount
                        {
                            CurrencyCode = currency,
                            Value = amountValue
                        }
                    }
                }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            
            var json = JsonSerializer.Serialize(orderRequest, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders");
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("Accept", "application/json");
            request.Content = content;

            var response = await httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create PayPal order: {response.StatusCode} - {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var orderResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var orderId = orderResponse.GetProperty("id").GetString();
            
            return orderId ?? throw new InvalidOperationException("Failed to create PayPal order");
        }

        public async Task<bool> CaptureOrderAsync(string orderId)
        {
            var token = await GetAccessTokenAsync();
            var isSandbox = configuration.GetValue<bool>("PayPal:UseSandbox", true);
            var baseUrl = isSandbox 
                ? "https://api.sandbox.paypal.com" 
                : "https://api.paypal.com";

            var httpClient = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders/{orderId}/capture");
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var captureResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);
                var status = captureResponse.GetProperty("status").GetString();
                return status == "COMPLETED";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to capture PayPal order: {response.StatusCode} - {errorContent}");
            }
        }
    }
}

