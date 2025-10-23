using System.Net.Http;
using System.Net.Http.Json;
using MyApp.BlazorUI.Components.Models;

namespace MyApp.BlazorUI.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- Generic API response wrapper ---
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
        }

        // --- Data paging wrapper (optional) ---
        public class UserData
        {
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public List<UserModel> Users { get; set; } = new();
        }

        // --- GET USERS ---
        public async Task<List<UserModel>> GetUsersAsync()
        {
            try
            {
                Console.WriteLine("📡 Fetching: api/Users");
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserData>>("api/Users");
                return response?.Data?.Users ?? new List<UserModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saat mengambil data user: {ex.Message}");
                return new List<UserModel>();
            }
        }

        // --- CREATE USER ---
        public async Task<bool> CreateUserAsync(UserModel user)
        {
            try
            {
                var createDto = new
                {
                    user.Name,
                    user.Email,
                    user.Password,
                    Role = user.Role.ToString(),
                    Status = (int)user.Status
                };

                var response = await _httpClient.PostAsJsonAsync("api/Users", createDto);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🔵 Create response: {content}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saat membuat user: {ex.Message}");
                return false;
            }
        }

        // --- UPDATE USER ---
        public async Task<bool> UpdateUserAsync(UserModel user)
        {
            try
            {
                var updateDto = new
                {
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(), // pastikan kirim string seperti "Admin"
                    Status = user.Status         // enum tetap boleh
                };

                var response = await _httpClient.PutAsJsonAsync($"api/Users/{user.Id}", updateDto);
                var content = await response.Content.ReadAsStringAsync();

                var json = System.Text.Json.JsonSerializer.Serialize(updateDto);
                Console.WriteLine($"🟢 Payload JSON: {json}");
                Console.WriteLine($"🔵 Update response: {content}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saat update user: {ex.Message}");
                return false;
            }
        }


        // --- DELETE USER ---
        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Users/{id}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🔵 Delete response: {content}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saat delete user: {ex.Message}");
                return false;
            }
        }
    }
}
