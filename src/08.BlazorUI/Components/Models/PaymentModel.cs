using System.Text.Json.Serialization;

namespace MyApp.BlazorUI.Components.Models
{
  public class PaymentModel
  {
    [JsonPropertyName("paymentMethodId")] // ✅ samakan dengan JSON
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("logo")]
    public string Logo { get; set; } = "";

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))] // ✅ biar "Active"/"Inactive" bisa dikonversi ke enum
    public PaymentStatus Status { get; set; }
  }

  [JsonConverter(typeof(JsonStringEnumConverter))] // optional tapi aman
  public enum PaymentStatus
  {
    Active,
    Inactive
  }
}
