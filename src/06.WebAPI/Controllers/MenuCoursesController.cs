using Microsoft.AspNetCore.Mvc;
using MyApp.WebAPI.DTOs;
using MyApp.WebAPI.Models;
using MyApp.WebAPI.Services;

namespace MyApp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MenuCoursesController : ControllerBase
    {
        private readonly IMenuCourseService _menuCourseService;

        public MenuCoursesController(IMenuCourseService menuCourseService)
        {
            _menuCourseService = menuCourseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _menuCourseService.GetAllMenuCoursesAsync();
            return Ok(new ApiResponse<IEnumerable<MenuCourseDto>>
            {
                Success = true,
                Data = courses,
                Message = "Berhasil mengambil semua data menu course."
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _menuCourseService.GetMenuCourseByIdAsync(id);
            if (course is null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Menu course dengan ID {id} tidak ditemukan."
                });
            }
            return Ok(new ApiResponse<MenuCourseDto>
            {
                Success = true,
                Data = course,
                Message = "Berhasil mengambil data menu course."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateMenuCourseDto createDto)
        {
            var newCourse = await _menuCourseService.CreateMenuCourseAsync(createDto);
            var response = new ApiResponse<MenuCourseDto>
            {
                Success = true,
                Data = newCourse,
                Message = "Menu course berhasil dibuat."
            };
            return CreatedAtAction(nameof(GetById), new { id = newCourse.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateMenuCourseDto updateDto)
        {
            var updatedCourse = await _menuCourseService.UpdateMenuCourseAsync(id, updateDto);
            if (updatedCourse is null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Menu course dengan ID {id} tidak ditemukan."
                });
            }
            return Ok(new ApiResponse<MenuCourseDto>
            {
                Success = true,
                Data = updatedCourse,
                Message = "Menu course berhasil diperbarui."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _menuCourseService.DeleteMenuCourseAsync(id);
            if (!success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Menu course dengan ID {id} tidak ditemukan."
                });
            }
            return NoContent();
        }
    }
}