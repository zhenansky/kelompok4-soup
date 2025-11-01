namespace MyApp.BlazorUI.Components.Models
{
    public class CourseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public int CategoryId { get; set; }

        public DateTimeOffset? ScheduleDate { get; set; }

        public string? Schedule { get; set; }

        // assignment info
        public int? ScheduleAssignmentId { get; set; }
        public int? ScheduleId { get; set; }

        public int AvailableSlot { get; set; }
    }
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public int MenuCourseCount { get; set; }
    }

    public class ScheduleModel
    {
        public int Id { get; set; }
        public string Schedule { get; set; } = string.Empty;
    }

}
