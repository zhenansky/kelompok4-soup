using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using MyApp.BlazorUI.Services.Interfaces;
using MyApp.BlazorUI.DTOs.Auth;
using MyApp.BlazorUI.DTOs;

namespace MyApp.BlazorUI.Services
{
  public class AuthService : IAuthService
  {
    private readonly IAuthClient _client;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IHttpClientFactory _httpFactory;
    private readonly HttpClient _http;

    public AuthService(IAuthClient client,
                       ILocalStorageService localStorage,
                       AuthenticationStateProvider authStateProvider,
                       IHttpClientFactory httpFactory,
                       HttpClient http)
    {
      _client = client;
      _localStorage = localStorage;
      _authStateProvider = authStateProvider;
      _httpFactory = httpFactory;
      _http = http;
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
    {
      var res = await _client.LoginAsync(dto);
      if (res == null || !res.Success || res.Data == null) return res;

      try
      {
        await _localStorage.SetItemAsync("accessToken", res.Data.AccessToken);
        await _localStorage.SetItemAsync("refreshToken", res.Data.RefreshToken);
        await _localStorage.SetItemAsync("expiresAt", res.Data.ExpiresAt);
      }
      catch (InvalidOperationException)
      {

      }

      ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(res.Data.AccessToken);

      return res;
    }

    public async Task<ApiResponse<bool>> LogoutAsync()
    {
      var res = await _client.LogoutAsync();

      try
      {
        await _localStorage.RemoveItemAsync("accessToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        await _localStorage.RemoveItemAsync("expiresAt");
      }
      catch (InvalidOperationException)
      {

      }

      ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();

      return res;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto)
    {
      var res = await _client.RegisterAsync(dto);
      return res;
    }

    public async Task<ApiResponse<AuthResponseDto>> ResendConfirmationEmailAsync(string email)
    {
      var res = await _client.ResendConfirmationEmailAsync(email);
      return res;
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync()
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
        return new ApiResponse<AuthResponseDto> { Success = false, Message = "LocalStorage unavailable during prerender." };
      }

      if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
      {
        return new ApiResponse<AuthResponseDto>
        {
          Success = false,
          Message = "Token not found."
        };
      }

      var dto = new RefreshTokenRequestDto { AccessToken = accessToken, RefreshToken = refreshToken };
      var res = await _client.RefreshTokenAsync(dto);
      if (res == null || !res.Success || res.Data == null) return res;

      try
      {
        await _localStorage.SetItemAsync("accessToken", res.Data.AccessToken);
        await _localStorage.SetItemAsync("refreshToken", res.Data.RefreshToken);
        await _localStorage.SetItemAsync("expiresAt", res.Data.ExpiresAt);
      }
      catch (InvalidOperationException)
      {
        // Ignore during prerender
      }

      ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(res.Data.AccessToken);
      return res;
    }

    public async Task<bool> IsUserAuthenticatedAsync()
    {
      try
      {
        var token = await _localStorage.GetItemAsync<string>("accessToken");
        return !string.IsNullOrEmpty(token);
      }
      catch (InvalidOperationException)
      {
        return false;
      }
    }

    public async Task<ApiResponse<AuthResponseDto>> ForgotPasswordAsync(ForgotPasswordRequestDto dto)
    {
      var res = await _client.ForgotPasswordAsync(dto);
      return res;
    }

    public async Task<ApiResponse<AuthResponseDto>> ResetPasswordAsync(ResetPasswordRequestDto dto)
    {
      var res = await _client.ResetPasswordAsync(dto);
      return res;
    }
  }
}