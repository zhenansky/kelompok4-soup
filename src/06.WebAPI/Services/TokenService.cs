using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyApp.WebAPI.Configuration;
using MyApp.WebAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyApp.WebAPI.Services
{
  public interface ITokenService
  {
    Task<string> CreateTokenAsync(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<bool> IsTokenValidAsync(string token);


  }

  public class TokenService : ITokenService
  {
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<User> _userManager;

    public TokenService(JwtSettings jwtSettings, UserManager<User> userManager)
    {
      _jwtSettings = jwtSettings;
      _userManager = userManager;
    }

    public async Task<string> CreateTokenAsync(User user)
    {
      var userRoles = await _userManager.GetRolesAsync(user);

      var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim("name", user.Name),
        new Claim("status", user.Status.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

      foreach (var role in userRoles)
      {
        claims.Add(new Claim(ClaimTypes.Role, role));
        claims.Add(new Claim("role", role)); // ✅ untuk FE
      }

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          issuer: _jwtSettings.Issuer,
          audience: _jwtSettings.Audience,
          claims: claims,
          expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate Refresh Token
    /// Token random untuk memperbarui access token yang expired
    /// </summary>
    public string GenerateRefreshToken()
    {
      var randomNumber = new byte[32];
      using var rng = RandomNumberGenerator.Create();
      rng.GetBytes(randomNumber);
      return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Get Principal from Expired Token
    /// Extract user claims dari expired token untuk proses refresh
    /// </summary>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
      var tokenValidationParameters = new TokenValidationParameters
      {
        ValidateAudience = _jwtSettings.ValidateAudience,
        ValidateIssuer = _jwtSettings.ValidateIssuer,
        ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
        ValidateLifetime = false, // Tidak validate lifetime karena expired
        ValidIssuer = _jwtSettings.Issuer,
        ValidAudience = _jwtSettings.Audience
      };

      var tokenHandler = new JwtSecurityTokenHandler();

      try
      {
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
          throw new SecurityTokenException("Invalid token");
        }

        return principal;
      }
      catch (Exception ex)
      {
        return null;
      }
    }

    /// <summary>
    /// Validate Token
    /// Cek apakah token valid dan belum expired
    /// </summary>
    public async Task<bool> IsTokenValidAsync(string token)
    {
      try
      {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        var tokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = _jwtSettings.ValidateIssuer,
          ValidIssuer = _jwtSettings.Issuer,
          ValidateAudience = _jwtSettings.ValidateAudience,
          ValidAudience = _jwtSettings.Audience,
          ValidateLifetime = true,
          ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkew)
        };

        tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
        return await Task.FromResult(true);
      }
      catch (Exception ex)
      {
        return await Task.FromResult(false);
      }
    }
  }
}
