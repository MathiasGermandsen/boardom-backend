using System.Text.Json;

namespace boardomapi.Services;

public class Auth0TokenService
{
    private readonly HttpClient? _httpClient;
    private readonly IConfiguration? _configuration;

    public Auth0TokenService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string?> GetTokenForArduinoAsync(string userId)
    {
        var domain = _configuration?["Auth0:Domain"];
        var clientId = _configuration?["Auth0:ClientId"];
        var clientSecret = _configuration?["Auth0:ClientSecret"];
        var audience = _configuration?["Auth0:Audience"];

        var tokenEndpoint = $"https://{domain}/oauth/token";

        var request = new
        {
            client_id = clientId,
            client_secret = clientSecret,
            audience = audience,
            grant_type = "client_credentials",
            scope = $"device:{userId}"
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(tokenEndpoint, request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Auth0TokenResponse>(json);
            return result?.AccessToken;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Auth0 token error: {ex.Message}");
            return null;
        }
    }
    }

    public record Auth0TokenResponse(
            [property: System.Text.Json.Serialization.JsonPropertyName("access_token")]
        string AccessToken,
            [property: System.Text.Json.Serialization.JsonPropertyName("token_type")]
        string TokenType,
            [property: System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        int ExpiresIn,
            [property: System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        string? RefreshToken = null
    );