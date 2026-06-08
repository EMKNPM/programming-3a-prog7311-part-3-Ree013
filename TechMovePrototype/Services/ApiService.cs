using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TechMovePrototype.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object body);
    Task<T?> PutAsync<T>(string endpoint, object body);
    Task PatchAsync(string endpoint, object body);
    Task DeleteAsync(string endpoint);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private string? _jwtToken;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    // Gets a JWT token from the API and caches it
    // Called automatically before any request if token is missing
    private async Task EnsureTokenAsync()
    {
        if (!string.IsNullOrEmpty(_jwtToken)) return;

        try
        {
            var loginDto = new { Username = "admin", Password = "admin123" };
            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TokenResponse>(
                    responseJson, _jsonOptions);
                _jwtToken = result?.Token;

                // Set the token on the HttpClient default headers
                // so ALL future requests include it automatically
                if (!string.IsNullOrEmpty(_jwtToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer", _jwtToken);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] Login failed: {ex.Message}");
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            await EnsureTokenAsync();
            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode) return default;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] GET {endpoint} failed: {ex.Message}");
            return default;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object body)
    {
        try
        {
            await EnsureTokenAsync();
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ApiService] POST {endpoint} failed: " +
                    $"{(int)response.StatusCode} - {errorBody}");
                return default;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] POST {endpoint} failed: {ex.Message}");
            return default;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object body)
    {
        try
        {
            await EnsureTokenAsync();
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            if (!response.IsSuccessStatusCode) return default;
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] PUT {endpoint} failed: {ex.Message}");
            return default;
        }
    }

    public async Task PatchAsync(string endpoint, object body)
    {
        try
        {
            await EnsureTokenAsync();
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
            {
                Content = content
            };
            await _httpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] PATCH {endpoint} failed: {ex.Message}");
        }
    }

    public async Task DeleteAsync(string endpoint)
    {
        try
        {
            await EnsureTokenAsync();
            await _httpClient.DeleteAsync(endpoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] DELETE {endpoint} failed: {ex.Message}");
        }
    }
}

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
}