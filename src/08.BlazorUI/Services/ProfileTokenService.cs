using Blazored.LocalStorage;
using System.IdentityModel.Tokens.Jwt;
using MyApp.BlazorUI.DTOs;

namespace MyApp.BlazorUI.Services
{
    public class ProfileTokenService
    {
        private readonly ILocalStorageService _localStorage;

        public ProfileTokenService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<UserProfileDto?> GetProfileFromTokenAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("accessToken");
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var username = jwt.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name")?.Value;
            var role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "User";
            var status = jwt.Claims.FirstOrDefault(c => c.Type == "status")?.Value ?? "Active";

            return new UserProfileDto
            {
                Email = email ?? "-",
                Username = username ?? "-",
                Role = role,
                Status = status
            };
        }
    }
}
