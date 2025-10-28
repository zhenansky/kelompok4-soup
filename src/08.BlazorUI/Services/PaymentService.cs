using Microsoft.AspNetCore.Components.Forms;
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

        public async Task<PaymentModel> CreatePaymentAsync(PaymentModel payment, IBrowserFile logoFile)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(payment.Name ?? ""), "Name");
            content.Add(new StringContent(payment.Status.ToString()), "Status");


            if (logoFile != null)
            {
                var stream = logoFile.OpenReadStream(5 * 1024 * 1024); // max 5MB
                content.Add(new StreamContent(stream), "LogoFile", logoFile.Name);
            }

            var response = await _http.PostAsync(Endpoint, content);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentModel>>();
            return apiResponse!.Data;
        }

        public async Task<PaymentModel> UpdatePaymentAsync(PaymentModel payment, IBrowserFile? logoFile)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(payment.Name ?? ""), "Name");
            content.Add(new StringContent(payment.Status.ToString()), "Status");

            if (logoFile != null)
            {
                var stream = logoFile.OpenReadStream(5 * 1024 * 1024); // max 5MB
                content.Add(new StreamContent(stream), "LogoFile", logoFile.Name);
            }

            var response = await _http.PutAsync($"{Endpoint}/{payment.Id}", content);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentModel>>();
            return apiResponse!.Data;
        }


        public async Task<bool> DeletePaymentAsync(int id)
        {
            var response = await _http.DeleteAsync($"{Endpoint}/{id}");
            return response.IsSuccessStatusCode;
        }

        public string GetFullImageUrl(string logoPath)
        {
            if (string.IsNullOrEmpty(logoPath)) return string.Empty;
            if (logoPath.StartsWith("http")) return logoPath;

            var baseUrl = "http://localhost:5099";
            // kalau tidak ada folder di path, tambahkan "payment-logos/"
            if (!logoPath.Contains("/"))
                logoPath = $"payment-logos/{logoPath}";

            return $"{baseUrl.TrimEnd('/')}/{logoPath.TrimStart('/')}";
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; } = default!;
    }
}
