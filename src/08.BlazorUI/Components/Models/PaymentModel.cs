using System.Text.Json.Serialization;

namespace MyApp.BlazorUI.Components.Models
{
  public class PaymentModel
  {
    [JsonPropertyName("paymentMethodId")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("logo")]
    public string Logo { get; set; } = "";

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentStatus Status { get; set; }
  }

  [JsonConverter(typeof(JsonStringEnumConverter))] 
  public enum PaymentStatus
  {
    Active,
    Inactive
  }
}
