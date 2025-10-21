using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using MyApp.BlazorUI.Components.Models;

namespace MyApp.BlazorUI.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public UserService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        // Generic API Response
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
        }

        // Data untuk list user
        public class UserData
        {
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public List<UserModel> Users { get; set; } = new();
        }

        // Ambil semua user
        public async Task<List<UserModel>> GetUsersAsync()
        {
            try
            {
                var baseUrl = _config["ApiBaseUrl"];
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserData>>($"{baseUrl}/users");
                return response?.Data?.Users ?? new List<UserModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saat mengambil data user: {ex.Message}");
                return new List<UserModel>();
            }
        }

        // Tambah user
        public async Task<bool> CreateUserAsync(UserModel user)
        {
            try
            {
                var baseUrl = _config["ApiBaseUrl"];
                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/users", user);

                if (!response.IsSuccessStatusCode) return false;

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserData>>();
                return result?.Success ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saat membuat user: {ex.Message}");
                return false;
            }
        }

        // Update user
        public async Task<bool> UpdateUserAsync(UserModel user)
        {
            try
            {
                Console.WriteLine($"🟡 Attempting update user {user.Id} status: {user.Status}");
                var baseUrl = _config["ApiBaseUrl"];
                var response = await _httpClient.PutAsJsonAsync($"{baseUrl}/users/{user.Id}", user);

                Console.WriteLine($"🟢 Response: {response.StatusCode}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🔵 Response body: {content}");

                if (!response.IsSuccessStatusCode) return false;

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserData>>();
                return result?.Success ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saat update user: {ex.Message}");
                return false;
            }
        }

        // Delete user
        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var baseUrl = _config["ApiBaseUrl"];
                var response = await _httpClient.DeleteAsync($"{baseUrl}/users/{id}");

                if (!response.IsSuccessStatusCode) return false;

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserData>>();
                return result?.Success ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saat delete user: {ex.Message}");
                return false;
            }
        }
    }
}
