using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using MyApp.BlazorUI.Helpers;

namespace MyApp.BlazorUI.Services
{
  public class CustomAuthStateProvider : AuthenticationStateProvider
  {
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private readonly AuthenticationState _anonymous;

    public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
    {
      _localStorage = localStorage;
      _httpClient = httpClient;
      _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
      try
      {
        var token = await _localStorage.GetItemAsync<string>("accessToken");

        if (string.IsNullOrWhiteSpace(token))
          return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = JwtParser.ParseClaimsFromJwt(token);
        if (!claims.Any())
        {
          Console.WriteLine("⚠️ No valid claims from token");
          return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ AuthStateProvider error: {ex.Message}");
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
      }
    }


    public void NotifyUserAuthentication(string token)
    {
      var claims = ParseClaimsFromJwt(token);
      var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
      var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
      NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
      var authState = Task.FromResult(_anonymous);
      NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
      var handler = new JwtSecurityTokenHandler();
      var token = handler.ReadJwtToken(jwt);
      return token.Claims;
    }
  }
}