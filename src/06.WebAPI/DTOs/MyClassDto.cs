namespace MyApp.WebAPI.DTOs
{
  public class MyClassDTO
  {
    public int MenuCourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTimeOffset Schedule { get; set; }
  }
}