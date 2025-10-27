using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyApp.BlazorUI.Helpers
{
  public static class JwtParser
  {
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
      if (string.IsNullOrWhiteSpace(jwt))
        return Enumerable.Empty<Claim>();

      try
      {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(jwt))
        {
          Console.WriteLine("⚠️ Invalid JWT format in JwtParser.");
          return Enumerable.Empty<Claim>();
        }

        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"⚠️ JwtParser failed: {ex.Message}");
        return Enumerable.Empty<Claim>();
      }
    }
  }
}
