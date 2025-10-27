using System.Net.Http.Headers;
using System.Text.Json;
using MyApp.BlazorUI.Models;
using MyApp.BlazorUI.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MyApp.BlazorUI.Services
{
  public class InvoiceService
  {
    private readonly HttpClient _httpClient;

    public InvoiceService(HttpClient httpClient)
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

    // 🔹 Ambil semua invoice (daftar)
    public async Task<List<InvoiceItem>> GetInvoicesAsync(string? token = null)
    {
      try
      {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/Invoices?pageNumber=1&pageSize=10");

        if (!string.IsNullOrWhiteSpace(token))
          request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
          Console.WriteLine($"❌ Gagal ambil daftar invoice: {response.StatusCode}");
          return new List<InvoiceItem>();
        }

        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🧾 Invoice JSON Response: {json}");

        var wrapped = JsonSerializer.Deserialize<InvoiceResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return wrapped?.Data?.Items ?? new List<InvoiceItem>();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Error fetching invoices: {ex.Message}");
        return new List<InvoiceItem>();
      }
    }

    public async Task<List<InvoiceItem>> GetInvoicesUserAsync(string? token = null)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(token))
        {
          Console.WriteLine("🚫 Token not provided.");
          return new List<InvoiceItem>();
        }

        // ✅ Extract userId from token
        var userId = GetUserIdFromToken(token);
        if (userId == null)
        {
          Console.WriteLine("⚠️ Could not extract userId from token.");
          return new List<InvoiceItem>();
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/Invoices/user/{userId}");

        if (!string.IsNullOrWhiteSpace(token))
          request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
          Console.WriteLine($"❌ Gagal ambil daftar invoice: {response.StatusCode}");
          return new List<InvoiceItem>();
        }

        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🧾 Invoice JSON Response: {json}");

        var wrapped = JsonSerializer.Deserialize<InvoiceResponseUser>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return wrapped?.Data ?? new List<InvoiceItem>();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Error fetching invoices: {ex.Message}");
        return new List<InvoiceItem>();
      }
    }

    // 🔹 Ambil detail invoice
    public async Task<InvoiceDetailData?> GetInvoiceDetailAsync(int invoiceId, string? token = null)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(token))
        {
          Console.WriteLine("🚫 Token kosong sebelum kirim request detail invoice.");
          return null;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"api/Invoices/{invoiceId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Console.WriteLine($"🔎 Request detail invoice {invoiceId} dengan token prefix: {token.Substring(0, Math.Min(token.Length, 15))}...");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
          var serverResponse = await response.Content.ReadAsStringAsync();
          Console.WriteLine($"❌ Gagal ambil detail invoice. Status: {response.StatusCode}\nServer response: {serverResponse}");
          return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🧾 Invoice Detail Response: {json}");

        var invoiceResponse = JsonSerializer.Deserialize<InvoiceDetailResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return invoiceResponse?.Data;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Error fetching invoice detail: {ex.Message}");
        return null;
      }
    }

    // 🔹 Buat invoice
    public async Task<ApiResponse<object>?> CreateinvoiceAsync(CreateInvoiceDTO createInvoiceDTO, string? token = null)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(token))
        {
          Console.WriteLine("🚫 Token kosong sebelum kirim request create invoice.");
          return null;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/Invoices");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // ✅ Serialize DTO to JSON and attach to request body
        var jsonContent = JsonSerializer.Serialize(createInvoiceDTO);
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        Console.WriteLine($"🔎 Request create invoice dengan token prefix: {token.Substring(0, Math.Min(token.Length, 15))}...");
        Console.WriteLine($"📦 Body JSON: {jsonContent}");

        var response = await _httpClient.SendAsync(request);
        var serverResponse = await response.Content.ReadAsStringAsync();



        if (!response.IsSuccessStatusCode)
        {
          Console.WriteLine($"❌ Gagal membuat invoice. Status: {response.StatusCode}\nServer response: {serverResponse}");

          // Try to parse error details
          ErrorResponse? error = null;
          try
          {
            error = JsonSerializer.Deserialize<ErrorResponse>(
                serverResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
          }
          catch
          {
            Console.WriteLine("⚠️ Could not parse error response as ErrorResponse.");
          }

          if (error != null)
          {
            Console.WriteLine($"🧩 ErrorCode: {error.ErrorCode}");
            Console.WriteLine($"📢 Message: {error.Message}");
            Console.WriteLine($"📚 Details: {error.Details}");
          }

          // You can return the error so UI can show details
          return new ApiResponse<object>
          {
            Success = false,
            Message = error?.Message ?? "Unknown error occurred.",
            Data = error?.Details
          };
        }

        Console.WriteLine($"🧾 Create invoice Response: {serverResponse}");

        var invoiceResponse = JsonSerializer.Deserialize<ApiResponse<object>>(
           serverResponse,
           new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return invoiceResponse;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Error Create Invoice: {ex.Message}");
        return null;
      }
    }
  }
}
