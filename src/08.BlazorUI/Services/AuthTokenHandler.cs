using Blazored.LocalStorage;
using System.Net.Http.Headers;
using MyApp.BlazorUI.Services.Interfaces;
using MyApp.BlazorUI.DTOs.Auth;
using MyApp.BlazorUI.DTOs;
using System.Text.Json;
using Microsoft.JSInterop;

namespace MyApp.BlazorUI.Services
{
  public class AuthTokenHandler : DelegatingHandler
  {
    private readonly ILocalStorageService _localStorage;
    private readonly IServiceProvider _services;
    private readonly string _apiBaseUrl = "http://localhost:5099/";

    public AuthTokenHandler(ILocalStorageService localStorage, IServiceProvider services)
    {
      _localStorage = localStorage;
      _services = services;
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
      catch (InvalidOperationException ex)
      {
        Console.WriteLine($"⚠️ LocalStorage belum siap saat refresh token (interop). {ex.Message}");
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

      try
      {
        await Task.Delay(500);

        token = await _localStorage.GetItemAsync<string>("accessToken");
      }
      catch (InvalidOperationException ex)
      {
        Console.WriteLine($"⚠️ LocalStorage belum siap (interop error). {ex.Message}");
      }


      if (!string.IsNullOrEmpty(token))
      {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Console.WriteLine($"🔐 Token attached: {token.Substring(0, Math.Min(token.Length, 20))}...");
      }

      var response = await base.SendAsync(request, cancellationToken);

      if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
      {
        Console.WriteLine("🔁 Token expired — refreshing...");
        var refreshed = await RefreshTokenAsync();
        if (!refreshed)
        {
          await _localStorage.RemoveItemAsync("accessToken");
          await _localStorage.RemoveItemAsync("refreshToken");
          await _localStorage.RemoveItemAsync("expiresAt");
          Console.WriteLine("🚪 Token refresh failed — user logged out.");
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
