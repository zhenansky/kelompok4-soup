using System.IdentityModel.Tokens.Jwt;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using MyApp.BlazorUI.Helpers;

namespace MyApp.BlazorUI.Services
{
  public class JwtValidationResult
  {
    public bool IsValid { get; set; }
    public bool IsExpired { get; set; }
    public DateTime? Expiry { get; set; }
    public string? ErrorMessage { get; set; }
  }


  public class TokenService
  {
    private readonly AuthenticationState _anonymous;

    public TokenService()
    {
      _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public JwtValidationResult ValidateJwtToken(string? token)
    {
      if (string.IsNullOrWhiteSpace(token))
        return new JwtValidationResult
        {
          IsValid = false,
          ErrorMessage = "Token is null or empty."
        };

      try
      {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
          return new JwtValidationResult
          {
            IsValid = false,
            ErrorMessage = "Invalid token format (not a valid JWT)."
          };
        }

        var jwt = handler.ReadJwtToken(token);

        // ✅ Check expiration claim
        var exp = jwt.Payload.Expiration;
        if (!exp.HasValue)
          return new JwtValidationResult
          {
            IsValid = false,
            ErrorMessage = "Token has no 'exp' claim."
          };

        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(exp.Value).UtcDateTime;
        bool isExpired = DateTime.UtcNow >= expiryDate;

        // ✅ Optional: validate expected claims (only if relevant for your app)
        if (!jwt.Payload.ContainsKey("sub"))
          return new JwtValidationResult
          {
            IsValid = false,
            ErrorMessage = "Token missing 'sub' (subject) claim."
          };

        if (jwt.Issuer is null || string.IsNullOrWhiteSpace(jwt.Issuer))
          return new JwtValidationResult
          {
            IsValid = false,
            ErrorMessage = "Token missing 'iss' (issuer) claim."
          };

        // ✅ If all good
        return new JwtValidationResult
        {
          IsValid = true,
          IsExpired = isExpired,
          Expiry = expiryDate
        };
      }
      catch (Exception ex)
      {
        return new JwtValidationResult
        {
          IsValid = false,
          ErrorMessage = $"Invalid token format: {ex.Message}"
        };
      }
    }


    public async Task<AuthenticationState> GetCustomAuthenticationStateAsync(string token)
    {
      try
      {
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
  }
}