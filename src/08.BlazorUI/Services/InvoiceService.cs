using System.Net.Http.Headers;
using System.Text.Json;
using MyApp.BlazorUI.Models;

namespace MyApp.BlazorUI.Services
{
    public class InvoiceService
    {
        private readonly HttpClient _httpClient;

        public InvoiceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 🔹 Ambil semua invoice (daftar)
        public async Task<List<InvoiceItem>> GetInvoicesAsync(string? token = null)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "api/Invoices?pageNumber=1&pageSize=10");

                if (!string.IsNullOrWhiteSpace(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Gagal ambil daftar invoice: {response.StatusCode}");
                    return new List<InvoiceItem>();
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🧾 Invoice JSON Response: {json}");

                var wrapped = JsonSerializer.Deserialize<InvoiceResponse>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return wrapped?.Data?.Items ?? new List<InvoiceItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching invoices: {ex.Message}");
                return new List<InvoiceItem>();
            }
        }

        // 🔹 Ambil detail invoice (revisi)
        public async Task<InvoiceDetailModel?> GetInvoiceDetailAsync(int invoiceId, string? token = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    Console.WriteLine("🚫 Token kosong sebelum kirim request detail invoice.");
                    return null;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"api/Invoices/{invoiceId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                Console.WriteLine($"🔎 Request detail invoice {invoiceId} dengan token prefix: {token.Substring(0, Math.Min(token.Length, 15))}...");

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var serverResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Gagal ambil detail invoice. Status: {response.StatusCode}\nServer response: {serverResponse}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🧾 Invoice Detail Response: {json}");

                var invoiceResponse = JsonSerializer.Deserialize<InvoiceDetailResponse>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var data = invoiceResponse?.Data;
                if (data == null)
                {
                    Console.WriteLine("⚠️ Tidak ada data pada respons invoice detail.");
                    return null;
                }

                // ✅ Mapping manual ke model untuk UI
                var model = new InvoiceDetailModel
                {
                    UserIdRef = data.UserIdRef,
                    InvoiceId = data.InvoiceId,
                    NoInvoice = data.NoInvoice,
                    Date = data.Date,
                    TotalPrice = data.TotalPrice,
                    ListCourse = data.ListCourse.Select(x => new MenuCourseModel
                    {
                        MenuCourseId = x.MenuCourseId,
                        Name = x.Name,
                        Category = x.Category,
                        ScheduleDate = x.ScheduleDate,
                        Price = x.Price
                    }).ToList()
                };

                Console.WriteLine($"✅ Invoice loaded: {model.NoInvoice}, total: {model.TotalPrice}, items: {model.ListCourse.Count}");
                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching invoice detail: {ex.Message}");
                return null;
            }
        }
    }
}
