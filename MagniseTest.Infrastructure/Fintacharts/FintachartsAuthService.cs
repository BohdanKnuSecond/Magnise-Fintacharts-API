using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MagniseTest.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MagniseTest.Infrastructure.Fintacharts
{
    public class FintachartsAuthService : IFintachartsAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string? _cachedToken; 

        public FintachartsAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync()
        {
           
            if (!string.IsNullOrEmpty(_cachedToken))
            {
                return _cachedToken;
            }

           
            var requestData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", "app-cli" },
            { "username", _configuration["Fintacharts:Username"]! }, 
            { "password", _configuration["Fintacharts:Password"]! }  
        };

            var content = new FormUrlEncodedContent(requestData);

            var response = await _httpClient.PostAsync("https://platform.fintacharts.com/identity/realms/fintatech/protocol/openid-connect/token", content);

            response.EnsureSuccessStatusCode(); 
           
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

            _cachedToken = tokenResponse?.AccessToken ?? throw new Exception("no token");

            return _cachedToken;
        }

   
        private class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;
        }

    }
}
