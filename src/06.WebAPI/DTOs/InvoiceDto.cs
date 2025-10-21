namespace MyApp.WebAPI.DTOs
{
  public class InvoiceDTO
  {
    public int InvoiceId { get; set; }
    public string NoInvoice { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int TotalCourse { get; set; }
    public decimal TotalPrice { get; set; }

    public string Email { get; set; } = string.Empty;
  }

  public class DetailInvoiceDTO
  {
    public int UserIdRef { get; set; }
    public int InvoiceId { get; set; }
    public string NoInvoice { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal TotalPrice { get; set; }
    public List<CourseItemDTO> ListCourse { get; set; } = new List<CourseItemDTO>();

  }

  public class CourseItemDTO
  {
    public int MenuCourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime ScheduleDate { get; set; }
    public decimal Price { get; set; }
  }

  public class CreateInvoiceDTO
  {
    public List<int> MSId { get; set; } = new List<int>();
  }
}