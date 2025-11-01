public class CartItem
{
  public int MSId { get; set; }
  public int MenuCourseId { get; set; }
  public int ScheduleId { get; set; }
  public string? Category { get; set; }
  public string? Name { get; set; }
  public DateTimeOffset Schedule { get; set; }
  public decimal Price { get; set; }
  public string? ImageUrl { get; set; }
  public bool Selected { get; set; }
}