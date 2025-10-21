using System.Text.Json.Serialization;

namespace MyApp.BlazorUI.Models
{
    public class InvoiceResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public InvoiceData? Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    public class InvoiceData
    {
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalRecords")]
        public int TotalRecords { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("items")]
        public List<InvoiceItem> Items { get; set; } = new();
    }

    public class InvoiceItem
    {
        [JsonPropertyName("invoiceId")]
        public int InvoiceId { get; set; }

        [JsonPropertyName("noInvoice")]
        public string NoInvoice { get; set; } = "";

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("totalCourse")]
        public int TotalCourse { get; set; }

        [JsonPropertyName("totalPrice")]
        public decimal TotalPrice { get; set; }

        // ✅ Email langsung dari backend
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }

    public class InvoiceDetailModel
    {
        [JsonPropertyName("invoiceId")]
        public int InvoiceId { get; set; }

        [JsonPropertyName("noInvoice")]
        public string NoInvoice { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("totalPrice")]
        public decimal TotalPrice { get; set; }

        [JsonPropertyName("listCourse")]
        public List<InvoiceCourseModel> Courses { get; set; } = new();
    }

    public class InvoiceCourseModel
    {
        [JsonPropertyName("name")]
        public string CourseName { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("scheduleDate")]
        public DateTime Schedule { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }
}
