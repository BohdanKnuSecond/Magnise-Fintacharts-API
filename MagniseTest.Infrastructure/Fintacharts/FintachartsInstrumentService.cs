using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MagniseTest.Application.Interfaces;
using MagniseTest.Domain.Entities;

namespace MagniseTest.Infrastructure.Fintacharts
{
    public class FintachartsInstrumentService : IFintachartsInstrumentService
    {
        private readonly HttpClient _httpClient;
        private readonly IFintachartsAuthService _authService;

        public FintachartsInstrumentService(HttpClient httpClient, IFintachartsAuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<List<Asset>> GetInstrumentsAsync()
        {
            var token = await _authService.GetAccessTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("https://platform.fintacharts.com/api/instruments/v1/instruments?provider=oanda&kind=forex");

            response.EnsureSuccessStatusCode(); 

            var data = await response.Content.ReadFromJsonAsync<FintachartsInstrumentsResponse>();

            if (data?.Data == null) return new List<Asset>();

            var assets = new List<Asset>();
            foreach (var item in data.Data)
            {
                assets.Add(new Asset
                {
                    Id = Guid.NewGuid(), 
                    Symbol = item.Symbol,
                    Description = item.Description
                });
            }

            return assets;
        }

      
        private class FintachartsInstrumentsResponse
        {
            [JsonPropertyName("data")]
            public List<FintachartsInstrumentDto>? Data { get; set; }
        }

       
        private class FintachartsInstrumentDto
        {
            [JsonPropertyName("symbol")]
            public string Symbol { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;
        }
    }
}
