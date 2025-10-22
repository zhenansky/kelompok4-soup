using Blazored.LocalStorage;
using System.Net.Http.Headers;
using MyApp.BlazorUI.Services.Interfaces;
using MyApp.BlazorUI.DTOs.Auth;
using MyApp.BlazorUI.DTOs;
using System.Text.Json;

namespace MyApp.BlazorUI.Services
{
  public class AuthTokenHandler : DelegatingHandler
  {
    private readonly ILocalStorageService _localStorage;
    private readonly string _apiBaseUrl = "http://localhost:5099/";

    public AuthTokenHandler(ILocalStorageService localStorage)
    {
      _localStorage = localStorage;
    }

    // 🔁 Refresh token saat 401
    public async Task<bool> RefreshTokenAsync()
    {
      string? accessToken = null;
      string? refreshToken = null;

      try
      {
        accessToken = await _localStorage.GetItemAsync<string>("accessToken");
        refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
      }
      catch (InvalidOperationException)
      {
        Console.WriteLine("⚠️ LocalStorage belum siap saat refresh token.");
        return false;
      }

      if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
        return false;

      var dto = new RefreshTokenRequestDto { AccessToken = accessToken, RefreshToken = refreshToken };

      // 🧠 Gunakan HttpClient baru (tanpa handler ini)
      using var refreshClient = new HttpClient { BaseAddress = new Uri(_apiBaseUrl) };
      var res = await refreshClient.PostAsJsonAsync("api/Auth/refresh-token", dto);

      if (!res.IsSuccessStatusCode)
        return false;

      var json = await res.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
      if (json?.Data?.AccessToken is not string token)
        return false;

      await _localStorage.SetItemAsync("accessToken", token);
      await _localStorage.SetItemAsync("refreshToken", json.Data.RefreshToken);
      await _localStorage.SetItemAsync("expiresAt", json.Data.ExpiresAt);
      Console.WriteLine("✅ Token berhasil diperbarui.");
      return true;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      string? token = null;

      for (int i = 0; i < 10; i++)
      {
        try
        {
          token = await _localStorage.GetItemAsync<string>("accessToken");
          if (!string.IsNullOrEmpty(token))
            break;
        }
        catch (InvalidOperationException)
        {

        }
        await Task.Delay(300);
      }

      if (!string.IsNullOrEmpty(token))
      {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Console.WriteLine($"🔐 Token attached: {token.Substring(0, Math.Min(token.Length, 20))}...");
      }
      else
      {
        Console.WriteLine("🚫 Tidak ada token di LocalStorage (mungkin belum login atau JS belum siap).");
      }

      var response = await base.SendAsync(request, cancellationToken);

      if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
      {
        Console.WriteLine("⚠️ Dapat 401 Unauthorized, mencoba refresh token...");
        var refreshed = await RefreshTokenAsync();
        if (!refreshed)
        {
          Console.WriteLine("❌ Refresh token gagal.");
          return response;
        }

        var newToken = await _localStorage.GetItemAsync<string>("accessToken");
        if (!string.IsNullOrEmpty(newToken))
        {
          request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
          Console.WriteLine($"✅ Token baru dipasang: {newToken.Substring(0, Math.Min(newToken.Length, 20))}...");
          response = await base.SendAsync(request, cancellationToken);
        }
      }

      return response;
    }
  }
}
