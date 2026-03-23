using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MagniseTest.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;



namespace MagniseTest.Infrastructure.WebSockets
{
    public class FintachartsWebSocketService : BackgroundService
    {
        private readonly IPriceStorage _priceStorage;
        private readonly IFintachartsAuthService _authService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FintachartsWebSocketService> _logger;
        private readonly Dictionary<string, string> _instrumentToSymbolMap = new();

        public FintachartsWebSocketService(
            IPriceStorage priceStorage,
            IFintachartsAuthService authService,
            IServiceScopeFactory scopeFactory,
            ILogger<FintachartsWebSocketService> logger)
        {
            _priceStorage = priceStorage;
            _authService = authService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConnectAndListenAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebSocket error. Retrying");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        private async Task ConnectAndListenAsync(CancellationToken stoppingToken)
        {
            var token = await _authService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return;

            using var ws = new ClientWebSocket();
            ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

            var uri = new Uri($"wss://platform.fintacharts.com/api/streaming/ws/v1/realtime?token={token}");

            await ws.ConnectAsync(uri, stoppingToken);

            using (var scope = _scopeFactory.CreateScope())
            {
                var assetRepository = scope.ServiceProvider.GetRequiredService<IAssetRepository>();
                var assets = await assetRepository.GetAllAssetsAsync();

                foreach (var asset in assets)
                {
                    _instrumentToSymbolMap[asset.Id.ToString().ToLower()] = asset.Symbol;

                    var subscribeMessage = new
                    {
                        type = "l1-subscription",
                        id = Guid.NewGuid().ToString(),
                        instrumentId = asset.Id.ToString(),
                        provider = asset.Provider,
                        subscribe = true,
                        kinds = new[] { "ask", "bid", "last" }
                    };

                    var jsonMessage = JsonSerializer.Serialize(subscribeMessage);
                    var bytes = Encoding.UTF8.GetBytes(jsonMessage);
                    await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, stoppingToken);
                }
            }

            var buffer = new byte[1024 * 8];
            while (ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                if (result.MessageType == WebSocketMessageType.Close) break;

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ParseAndUpdatePrice(message);
            }
        }

        private void ParseAndUpdatePrice(string jsonMessage)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonMessage);
                var root = doc.RootElement;

                if (root.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "l1-update")
                {
                    var instrumentId = root.GetProperty("instrumentId").GetString()?.ToLower();

                    if (root.TryGetProperty("last", out var lastProp) && lastProp.TryGetProperty("price", out var priceProp))
                    {
                        var price = priceProp.GetDecimal();

                        if (instrumentId != null && _instrumentToSymbolMap.TryGetValue(instrumentId, out var symbol))
                        {
                            _priceStorage.UpdatePrice(symbol, price, DateTime.UtcNow);
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}