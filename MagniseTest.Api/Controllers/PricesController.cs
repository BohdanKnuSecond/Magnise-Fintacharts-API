using MagniseTest.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace MagniseTest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricesController : ControllerBase
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IFintachartsAuthService _authService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPriceStorage _priceStorage;

        public PricesController(
            IAssetRepository assetRepository,
            IFintachartsAuthService authService,
            IHttpClientFactory httpClientFactory,
            IPriceStorage priceStorage)
        {
            _assetRepository = assetRepository;
            _authService = authService;
            _httpClientFactory = httpClientFactory;
            _priceStorage = priceStorage;
        }

        [HttpGet("realtime")]
        public IActionResult GetRealTimePrice([FromQuery] string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return BadRequest("Symbol is required.");

            var data = _priceStorage.GetPrice(symbol.ToUpper());

            if (data == null)
            {
                return Ok(new { message = $"No realtime data for {symbol} yet. Web-socket might still be connecting." });
            }

            return Ok(new
            {
                Symbol = symbol,
                Price = data.Value.Price,
                LastUpdated = data.Value.Timestamp
            });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] string symbol, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return BadRequest("Symbol is required.");

            if (from > to)
                return BadRequest("Invalid date range.");
            if (from > DateTime.UtcNow || to > DateTime.UtcNow) return BadRequest("Dates cannot be in the future.");

            try
            {
                var assets = await _assetRepository.GetAllAssetsAsync();
                var asset = assets.FirstOrDefault(a => a.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

                if (asset == null)
                    return NotFound($"Asset '{symbol}' not found in the database.");

                var token = await _authService.GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                    return StatusCode(500, "Failed to get access token from Fintacharts.");

                var provider = string.IsNullOrWhiteSpace(asset.Provider) ? "oanda" : asset.Provider;

                var url = $"https://platform.fintacharts.com/api/bars/v1/bars/date-range?" +
                          $"instrumentId={asset.Id}&" +
                          $"provider={provider}&" +
                          $"interval=1&" +
                          $"periodicity=day&" +
                          $"startDate={from:yyyy-MM-dd}&" +
                          $"endDate={to:yyyy-MM-dd}";

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, $"External API error: {content}");
                }

                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server Crash Details: {ex.Message} \n {ex.StackTrace}");
            }
        }
    }
}