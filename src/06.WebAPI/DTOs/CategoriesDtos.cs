namespace  MyApp.WebAPI.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string? Description { get; set; }
        public int MenuCourseCount { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
        public string? Description { get; set; }
    }
}