namespace MyApp.WebAPI.DTOs
{
  public class MenuCourseScheduleDto
  {
    public int Id { get; set; }
    public int AvailableSlot { get; set; }
    public string Status { get; set; } = string.Empty;

    public int MenuCourseId { get; set; }
    public string MenuCourseName { get; set; } = string.Empty;
    public int ScheduleId { get; set; }
    public DateTimeOffset ScheduleDate { get; set; }
  }

  public class CreateMenuCourseScheduleDto
  {
    public int AvailableSlot { get; set; }
    public string Status { get; set; } = "Active";
    public int MenuCourseId { get; set; }
    public int ScheduleId { get; set; }
  }

  public class UpdateMenuCourseScheduleDto
  {
    public int AvailableSlot { get; set; }
    public string Status { get; set; } = string.Empty;
  }
}