namespace GLMS.Web.Services
{
    // This is the ICurrencyStrategy interface we designed in Part 1
    public interface ICurrencyService
    {
        Task<decimal> GetUSDToZARRateAsync();
        decimal ConvertUSDToZAR(decimal amountUSD, decimal rate);
    }

    // This is the ExchangeRateAPIStrategy from Part 1
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;

        public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<decimal> GetUSDToZARRateAsync()
        {
            try
            {
                // Using the free open.er-api.com endpoint - no API key needed
                var response = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>(
                    "https://open.er-api.com/v6/latest/USD"
                );

                if (response != null && response.Rates.ContainsKey("ZAR"))
                {
                    return response.Rates["ZAR"];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch exchange rate, using fallback rate");
            }

            // Fallback rate if API is down - this is the FixedRateStrategy from Part 1
            return 18.50m;
        }

        // This is the actual conversion math that we unit test
        public decimal ConvertUSDToZAR(decimal amountUSD, decimal rate)
        {
            return Math.Round(amountUSD * rate, 2);
        }
    }

    // Simple response model for the API JSON
    public class ExchangeRateResponse
    {
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
