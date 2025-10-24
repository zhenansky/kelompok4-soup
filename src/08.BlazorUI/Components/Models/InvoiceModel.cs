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

  public class InvoiceResponseUser
  {
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public List<InvoiceItem>? Data { get; set; }

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

}
