// Lokasi: src/06.WebAPI/DTOs/ScheduleDtos.cs

namespace MyApp.WebAPI.DTOs
{
    public class ScheduleDto
    {
        public int Id { get; set; }
        public DateTimeOffset ScheduleDate { get; set; }
    }

    public class CreateScheduleDto
    {
        public DateTimeOffset ScheduleDate { get; set; }
    }

    public class UpdateScheduleDto
    {
        public DateTimeOffset ScheduleDate { get; set; }
    }
}