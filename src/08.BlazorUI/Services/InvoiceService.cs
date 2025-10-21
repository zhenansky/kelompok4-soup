using System.Net.Http.Headers;
using System.Net.Http.Json;
using MyApp.BlazorUI.Models;

namespace MyApp.BlazorUI.Services
{
    public class InvoiceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:5099/api"; // ✅ Base URL untuk API

        public InvoiceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ✅ Ambil semua invoice dari endpoint utama
        public async Task<List<InvoiceItem>> GetInvoicesAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Gunakan base URL biar lebih rapi
                var response = await _httpClient.GetAsync($"{_baseUrl}/Invoices?pageNumber=1&pageSize=10");
                response.EnsureSuccessStatusCode();

                // Deserialize sesuai struktur JSON backend (data → items)
                var result = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
                var invoices = result?.Data?.Items ?? new List<InvoiceItem>();

                // Tidak perlu ambil email lagi, karena sudah dikirim dari backend
                return invoices;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching invoices: {ex.Message}");
                return new List<InvoiceItem>();
            }
        }

        // ✅ Ambil total invoice
        public async Task<int> GetTotalInvoicesAsync(string token)
        {
            var invoices = await GetInvoicesAsync(token);
            return invoices.Count;
        }

        // ✅ Ambil detail invoice
        public async Task<InvoiceDetailModel?> GetInvoiceDetailAsync(int invoiceId, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{_baseUrl}/Invoices/{invoiceId}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<InvoiceDetailModel>();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching invoice detail: {ex.Message}");
                return null;
            }
        }
    }
}
