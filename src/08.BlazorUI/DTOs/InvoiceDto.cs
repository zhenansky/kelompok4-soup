using System.Text.Json.Serialization;

namespace MyApp.BlazorUI.DTOs
{
  public class CreateInvoiceDTO
  {
    [JsonPropertyName("MSId")]
    public List<int> MSId { get; set; } = new List<int>();
  }
}