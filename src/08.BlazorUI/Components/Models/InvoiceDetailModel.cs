using System.Text.Json.Serialization;

namespace MyApp.BlazorUI.Models
{
    // 🔹 RESPONSE UTAMA
    public class InvoiceDetailResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public InvoiceDetailData? Data { get; set; }
    }

    // 🔹 DATA INVOICE
    public class InvoiceDetailData
    {
        [JsonPropertyName("userIdRef")]
        public int UserIdRef { get; set; }

        [JsonPropertyName("invoiceId")]
        public int InvoiceId { get; set; }

        [JsonPropertyName("noInvoice")]
        public string NoInvoice { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("totalPrice")]
        public decimal TotalPrice { get; set; }

        [JsonPropertyName("listCourse")]
        public List<InvoiceCourseData> ListCourse { get; set; } = new();
    }

    // 🔹 DATA COURSE DALAM INVOICE
    public class InvoiceCourseData
    {
        [JsonPropertyName("menuCourseId")]
        public int MenuCourseId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("scheduleDate")]
        public DateTime ScheduleDate { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }

    // 🔹 MODEL UNTUK UI (setelah mapping)
    public class InvoiceDetailModel
    {
        public int UserIdRef { get; set; }
        public int InvoiceId { get; set; }
        public string NoInvoice { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal TotalPrice { get; set; }

        // ✅ Ganti ke nama JSON: listCourse
        public List<MenuCourseModel> ListCourse { get; set; } = new();
    }
    public class MenuCourseModel
    {
        public int MenuCourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime ScheduleDate { get; set; }
        public decimal Price { get; set; }
    }
}
