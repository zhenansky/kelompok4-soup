using System.Net.Http.Headers;
using System.Text.Json;
using MyApp.BlazorUI.Components.Models;
using System.IdentityModel.Tokens.Jwt;

namespace MyApp.BlazorUI.Services
{
  public class MyClassServices
  {
    private readonly HttpClient _httpClient;

    public MyClassServices(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    public static int? GetUserIdFromToken(string token)
    {
      try
      {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
            c.Type == "userId" || c.Type == "sub" || c.Type == "nameid" || c.Type == "uid");

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
          return userId;

        Console.WriteLine("⚠️ User ID claim not found in token.");
        return null;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Error decoding JWT: {ex.Message}");
        return null;
      }
    }

    public async Task<List<MyClassData>> GetMyClassAsync(string? token = null)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(token))
        {
          Console.WriteLine("🚫 Token not provided.");
          return new List<MyClassData>();
        }

        // ✅ Extract userId from token
        var userId = GetUserIdFromToken(token);
        if (userId == null)
        {
          Console.WriteLine("⚠️ Could not extract userId from token.");
          return new List<MyClassData>();
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/MyClasses/user/{userId}");

        if (!string.IsNullOrWhiteSpace(token))
          request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
          Console.WriteLine($"❌ Gagal ambil daftar My Class: {response.StatusCode}");
          return new List<MyClassData>();
        }

        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🧾 My Class JSON Response: {json}");

        var wrapped = JsonSerializer.Deserialize<MyClassResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return wrapped?.Data ?? new List<MyClassData>();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Error fetching My Class: {ex.Message}");
        return new List<MyClassData>();
      }
    }
  }
}
