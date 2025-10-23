using System.Text.Json.Serialization;

namespace MyApp.BlazorUI.Components.Models
{
  public class UserModel
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("role")]
    public UserRole Role { get; set; }

    [JsonPropertyName("status")]
    public UserStatus Status { get; set; }
  }

  public enum UserRole
  {
    Admin,
    User
  }

  public enum UserStatus
  {
    Active,
    Inactive
  }

  public class UpdateUserDto
  {
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }
  }


}