using Microsoft.AspNetCore.Mvc;
using MyApp.WebAPI.DTOs;
using MyApp.WebAPI.Models;
using MyApp.WebAPI.Services;

namespace MyApp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public SchedulesController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var schedules = await _scheduleService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<ScheduleDto>>
            {
                Success = true,
                Data = schedules,
                Message = "Berhasil mengambil semua data jadwal."
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var schedule = await _scheduleService.GetAsync(id);
            if (schedule == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Jadwal dengan ID {id} tidak ditemukan."
                });
            }
            return Ok(new ApiResponse<ScheduleDto>
            {
                Success = true,
                Data = schedule,
                Message = "Berhasil mengambil data jadwal."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateScheduleDto createDto)
        {
            var newSchedule = await _scheduleService.CreateAsync(createDto);
            var response = new ApiResponse<ScheduleDto>
            {
                Success = true,
                Data = newSchedule,
                Message = "Jadwal berhasil dibuat."
            };
            return CreatedAtAction(nameof(GetById), new { id = newSchedule.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateScheduleDto updateDto)
        {
            var updatedSchedule = await _scheduleService.UpdateAsync(id, updateDto);
            if (updatedSchedule == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Jadwal dengan ID {id} tidak ditemukan."
                });
            }
            return Ok(new ApiResponse<ScheduleDto>
            {
                Success = true,
                Data = updatedSchedule,
                Message = "Jadwal berhasil diperbarui."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _scheduleService.DeleteAsync(id);
            if (!success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Jadwal dengan ID {id} tidak ditemukan."
                });
            }
            return NoContent();
        }
    }
}