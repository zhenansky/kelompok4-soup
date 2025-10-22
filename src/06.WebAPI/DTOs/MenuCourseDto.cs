namespace   MyApp.WebAPI.DTOs
{
    public class MenuCourseDto
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty; 
        public string? Image { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty; 
    }

    public class CreateMenuCourseDto
    {
        public string Name { get; set; } = string.Empty; 
        public IFormFile? ImageFile { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
    }

    public class UpdateMenuCourseDto
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
    }
}