using HnHMapperServer.Web.Models;
using System.Net.Http.Json;

namespace HnHMapperServer.Web.Services;

public class UserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UserService> _logger;

    public UserService(IHttpClientFactory httpClientFactory, ILogger<UserService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<UserTokenDto>> GetUserTokensAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("API");
            var response = await client.GetFromJsonAsync<List<UserTokenDto>>("/api/user/tokens");
            return response ?? new List<UserTokenDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user tokens");
            return new List<UserTokenDto>();
        }
    }

    public async Task<TokenGenerationResponse?> GenerateTokenAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("API");
            var response = await client.PostAsync("/api/user/tokens", null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TokenGenerationResponse>();
            }
            else
            {
                _logger.LogWarning("Failed to generate token. Status: {StatusCode}", response.StatusCode);
                var error = await response.Content.ReadFromJsonAsync<TokenGenerationResponse>();
                return error;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token");
            return new TokenGenerationResponse
            {
                Success = false,
                Message = "An error occurred while generating the token"
            };
        }
    }

    public class TokenGenerationResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
