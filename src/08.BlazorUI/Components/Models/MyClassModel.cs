using System.Text.Json.Serialization;

namespace MyApp.BlazorUI.Components.Models
{
  public class MyClassResponse
  {
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public List<MyClassData>? Data { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
  }

  public class MyClassData
  {
    [JsonPropertyName("menuCourseId")]
    public int MenuCourseId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("schedule")]
    public DateTime Schedule { get; set; }
  }
}