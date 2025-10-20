using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using MyApp.BlazorUI.Components.Models;

namespace MyApp.BlazorUI.Services
{
    public class DashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public DashboardService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        // ==== STRUCTURE UNTUK RESPONSE API ====
        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
        }

        private class UserData
        {
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public List<UserModel> Users { get; set; } = new();
        }

        // ==== METHOD UNTUK DASHBOARD ====

        /// <summary>
        /// Mengambil total user aktif dari API Users
        /// </summary>
        public async Task<int> GetTotalActiveUsersAsync()
        {
            try
            {
                var baseUrl = _config["ApiBaseUrl"];
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserData>>($"{baseUrl}/users?page=1&pageSize=100");

                if (response?.Data?.Users == null)
                    return 0;

                // Asumsi status 0 = aktif
                return response.Data.Users.Count(u => u.Status == UserStatus.Active || u.Status == 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error mengambil total user aktif: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Mengambil total seluruh member (active + inactive)
        /// </summary>
        public async Task<int> GetTotalMembersAsync()
        {
            try
            {
                var baseUrl = _config["ApiBaseUrl"];
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserData>>($"{baseUrl}/users?page=1&pageSize=100");

                if (response?.Data?.Users == null)
                    return 0;

                // Menghitung semua user (baik aktif maupun tidak)
                return response.Data.Users.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error mengambil total member: {ex.Message}");
                return 0;
            }
        }
    }
}
