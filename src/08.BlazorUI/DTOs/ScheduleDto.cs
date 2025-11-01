namespace MyApp.BlazorUI.DTOs
{
    public class ScheduleDto
    {
        public int Id { get; set; }
        public int AvailableSlot { get; set; }
        public DateTimeOffset ScheduleDate { get; set; }
    }
}