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

        private const string Endpoint = "api/payment-methods"; // 🔧 endpoint baru disimpan di satu tempat

        public async Task<List<PaymentModel>> GetPaymentsAsync()
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<PaymentModel>>>(Endpoint);
            return response?.Data ?? new List<PaymentModel>();
        }

        public async Task<PaymentModel> CreatePaymentAsync(PaymentModel payment)
        {
            var response = await _http.PostAsJsonAsync(Endpoint, payment);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentModel>>();
            return apiResponse!.Data;
        }

        public async Task<PaymentModel> UpdatePaymentAsync(PaymentModel payment)
        {
            var response = await _http.PutAsJsonAsync($"{Endpoint}/{payment.Id}", payment);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentModel>>();
            return apiResponse!.Data;
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            var response = await _http.DeleteAsync($"{Endpoint}/{id}");
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
