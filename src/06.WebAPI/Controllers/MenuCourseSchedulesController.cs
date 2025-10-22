// Lokasi: src/06.WebAPI/Controllers/MenuCourseSchedulesController.cs
using Microsoft.AspNetCore.Mvc;
using MyApp.WebAPI.DTOs;
using MyApp.WebAPI.Models;
using MyApp.WebAPI.Services;

namespace MyApp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/menucourses")]
    [Produces("application/json")]
    public class MenuCourseSchedulesController : ControllerBase
    {
        private readonly IMenuCourseScheduleService _mcsService;

        public MenuCourseSchedulesController(IMenuCourseScheduleService mcsService)
        {
            _mcsService = mcsService;
        }

        [HttpGet("{menuCourseId}/schedules")]
        public async Task<IActionResult> GetSchedulesForCourse(int menuCourseId)
        {
            var schedules = await _mcsService.GetSchedulesForCourseAsync(menuCourseId);
            return Ok(new ApiResponse<IEnumerable<MenuCourseScheduleDto>>
            {
                Success = true,
                Data = schedules,
                Message = $"Berhasil mengambil jadwal untuk course ID {menuCourseId}."
            });
        }

        [HttpPost("schedules")]
        public async Task<IActionResult> AssignSchedule([FromForm] CreateMenuCourseScheduleDto createDto)
        {
            var newAssignment = await _mcsService.AssignScheduleAsync(createDto);
            var response = new ApiResponse<MenuCourseScheduleDto>
            {
                Success = true,
                Data = newAssignment,
                Message = "Jadwal berhasil ditambahkan ke course."
            };
            return StatusCode(201, response);
        }

        [HttpPut("schedules/{id}")]
        public async Task<IActionResult> UpdateAssignment(int id, [FromForm] UpdateMenuCourseScheduleDto updateDto)
        {
            var updatedAssignment = await _mcsService.UpdateAssignmentAsync(id, updateDto);

            if (updatedAssignment is null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Pendaftaran jadwal dengan ID {id} tidak ditemukan."
                });
            }

            return Ok(new ApiResponse<MenuCourseScheduleDto>
            {
                Success = true,
                Data = updatedAssignment,
                Message = "Detail jadwal berhasil diperbarui."
            });
        }

        [HttpDelete("schedules/{id}")]
        public async Task<IActionResult> UnassignSchedule(int id)
        {
            var success = await _mcsService.UnassignScheduleAsync(id);

            if (!success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Pendaftaran jadwal dengan ID {id} tidak ditemukan."
                });
            }

            return NoContent();
        }
    }
}