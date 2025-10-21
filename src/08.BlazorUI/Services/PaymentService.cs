using MyApp.BlazorUI.Components.Models;
using System.Net.Http.Json;

namespace MyApp.BlazorUI.Services
{
    public class PaymentService
    {
        private readonly HttpClient _http;

        public PaymentService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<PaymentModel>> GetPaymentsAsync()
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<PaymentModel>>>("api/PaymentMethod");
            return response?.Data ?? new List<PaymentModel>();
        }

        public async Task<PaymentModel> CreatePaymentAsync(PaymentModel payment)
        {
            var response = await _http.PostAsJsonAsync("api/PaymentMethod", payment);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentModel>>();
            return apiResponse!.Data;
        }

        public async Task<PaymentModel> UpdatePaymentAsync(PaymentModel payment)
        {
            var response = await _http.PutAsJsonAsync($"api/PaymentMethod/{payment.Id}", payment);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentModel>>();
            return apiResponse!.Data;
        }

        // Ubah parameter jadi int
        public async Task<bool> DeletePaymentAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/PaymentMethod/{id}");
            return response.IsSuccessStatusCode;
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; } = default!;
    }
}
