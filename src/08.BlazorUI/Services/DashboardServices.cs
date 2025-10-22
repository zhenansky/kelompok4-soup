using System.Net.Http.Json;
using MyApp.BlazorUI.Components.Models;

namespace MyApp.BlazorUI.Services
{
    public class DashboardService
    {
        private readonly HttpClient _httpClient;

        public DashboardService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            Console.WriteLine("📊 DashboardService initialized (no token needed).");
        }

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

        public async Task<int> GetTotalActiveUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserData>>("api/users?page=1&pageSize=100");

                if (response?.Data?.Users == null)
                    return 0;

                return response.Data.Users.Count(u => u.Status == UserStatus.Active || u.Status == 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error mengambil total user aktif: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTotalMembersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserData>>("api/users?page=1&pageSize=100");

                if (response?.Data?.Users == null)
                    return 0;

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
