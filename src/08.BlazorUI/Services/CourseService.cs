using System.Net.Http.Json;
using MyApp.BlazorUI.Components.Models;
using MyApp.BlazorUI.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace MyApp.BlazorUI.Services
{
    public class CourseService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public CourseService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        private string BaseUrl => _config["ApiBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5099";

        // ===============================
        // GET
        // ===============================
        public async Task<List<CourseModel>> GetCoursesAsync()
        {
            var wrapper = await _httpClient.GetFromJsonAsync<ApiResponse<List<MenuCourseDto>>>($"{BaseUrl}/api/menucourses");
            if (wrapper?.Data == null)
                return new List<CourseModel>();

            var courseDtos = wrapper.Data;
            var scheduleTasks = courseDtos.Select(async dto =>
            {
                var scheduleResponse = await _httpClient.GetFromJsonAsync<ApiResponse<List<MenuCourseScheduleDto>>>(
                    $"{BaseUrl}/api/menucourses/{dto.Id}/schedules");

                var firstSchedule = scheduleResponse?.Data?.FirstOrDefault();

                return new CourseModel
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    CategoryId = dto.CategoryId,
                    CategoryName = dto.CategoryName,
                    ImageUrl = string.IsNullOrEmpty(dto.Image) ? null : $"{BaseUrl}/{dto.Image.TrimStart('/')}",
                    Price = dto.Price,
                    Description = dto.Description ?? "-",
                    ScheduleDate = firstSchedule?.ScheduleDate,
                    Schedule = firstSchedule == null
                        ? "Belum dijadwalkan"
                        : firstSchedule.ScheduleDate.ToString("dddd, dd MMM yyyy HH:mm"),
                    AvailableSlot = firstSchedule?.AvailableSlot ?? 0,
                    ScheduleAssignmentId = firstSchedule?.Id,
                    ScheduleId = firstSchedule?.ScheduleId
                };
            });

            var courses = await Task.WhenAll(scheduleTasks);
            return courses.ToList();
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var wrapper = await _httpClient.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>($"{BaseUrl}/api/categories");
            return wrapper?.Data ?? new List<CategoryDto>();
        }

        // ===============================
        // CREATE
        // ===============================
        public async Task<bool> AddCourseAsync(CreateCourseDto dto, IBrowserFile? file)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(dto.Name ?? ""), "Name");
            content.Add(new StringContent(dto.Description ?? ""), "Description");
            content.Add(new StringContent(dto.Price.ToString()), "Price");
            content.Add(new StringContent(dto.CategoryId.ToString()), "CategoryId");

            if (file != null)
            {
                var stream = file.OpenReadStream(10 * 1024 * 1024);
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "ImageFile", file.Name);
            }

            // 1️⃣ Buat course dulu
            var response = await _httpClient.PostAsync($"{BaseUrl}/api/menucourses", content);
            if (!response.IsSuccessStatusCode)
                return false;

            var created = await response.Content.ReadFromJsonAsync<ApiResponse<MenuCourseDto>>();
            if (created?.Data == null)
                return false;

            var courseId = created.Data.Id;

            // 2️⃣ Buat schedule (jika ada)
            if (dto.ScheduleDate.HasValue && dto.AvailableSlot > 0)
            {
                // ✅ Gunakan form-data, bukan JSON
                var schedulePayload = new
                {
                    ScheduleDate = dto.ScheduleDate,
                    MenuCourseId = courseId
                };

                var createScheduleResponse = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/schedules", schedulePayload);
                if (!createScheduleResponse.IsSuccessStatusCode)
                    return false;

                var createdSchedule = await createScheduleResponse.Content.ReadFromJsonAsync<ApiResponse<ScheduleDto>>();
                if (createdSchedule?.Data == null)
                    return false;

                var scheduleId = createdSchedule.Data.Id;

                // 3️⃣ Assign schedule ke course
                using var assignContent = new MultipartFormDataContent();
                assignContent.Add(new StringContent(dto.AvailableSlot.ToString()), "AvailableSlot");
                assignContent.Add(new StringContent("Active"), "Status");
                assignContent.Add(new StringContent(courseId.ToString()), "MenuCourseId");
                assignContent.Add(new StringContent(scheduleId.ToString()), "ScheduleId");

                var assignResponse = await _httpClient.PostAsync($"{BaseUrl}/api/menucourses/schedules", assignContent);
                if (!assignResponse.IsSuccessStatusCode)
                    return false;
            }

            return true;
        }


        // ===============================
        // UPDATE
        // ===============================
        public async Task<bool> UpdateCourseAsync(int id, CreateCourseDto dto, IBrowserFile? file, int? scheduleId = null, int? scheduleAssignmentId = null)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.Name ?? ""), "Name");
            content.Add(new StringContent(dto.Description ?? ""), "Description");
            content.Add(new StringContent(dto.Price.ToString()), "Price");
            content.Add(new StringContent(dto.CategoryId.ToString()), "CategoryId");

            if (file != null)
            {
                var stream = file.OpenReadStream(10 * 1024 * 1024);
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "ImageFile", file.Name);
            }

            // 1️⃣ Update data utama course
            var updateResponse = await _httpClient.PutAsync($"{BaseUrl}/api/menucourses/{id}", content);
            if (!updateResponse.IsSuccessStatusCode)
                return false;

            // 2️⃣ Tangani schedule
            if (dto.ScheduleDate.HasValue && dto.AvailableSlot > 0)
            {
                int finalScheduleId;

                if (scheduleId.HasValue && scheduleId.Value > 0)
                {
                    // Update existing schedule
                    var updatePayload = new
                    {
                        ScheduleDate = dto.ScheduleDate
                    };


                    var updateScheduleResponse = await _httpClient.PutAsJsonAsync($"{BaseUrl}/api/schedules/{scheduleId.Value}", updatePayload);
                    if (!updateScheduleResponse.IsSuccessStatusCode)
                        return false;

                    finalScheduleId = scheduleId.Value;
                }
                else
                {
                    // Buat schedule baru
                    var schedulePayload = new
                    {
                        ScheduleDate = dto.ScheduleDate,
                    };



                    var createScheduleResponse = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/schedules", schedulePayload);
                    if (!createScheduleResponse.IsSuccessStatusCode)
                        return false;

                    var createdSchedule = await createScheduleResponse.Content.ReadFromJsonAsync<ApiResponse<ScheduleDto>>();
                    if (createdSchedule?.Data == null)
                        return false;

                    finalScheduleId = createdSchedule.Data.Id;
                }

                // 3️⃣ Assign / Update assignment
                using var assignContent = new MultipartFormDataContent();
                assignContent.Add(new StringContent(dto.AvailableSlot.ToString()), "AvailableSlot");
                assignContent.Add(new StringContent("Active"), "Status");
                assignContent.Add(new StringContent(id.ToString()), "MenuCourseId");
                assignContent.Add(new StringContent(finalScheduleId.ToString()), "ScheduleId");

                if (scheduleAssignmentId.HasValue && scheduleAssignmentId.Value > 0)
                {
                    var assignResponse = await _httpClient.PutAsync($"{BaseUrl}/api/menucourses/schedules/{scheduleAssignmentId.Value}", assignContent);
                    if (!assignResponse.IsSuccessStatusCode)
                        return false;
                }
                else
                {
                    var assignResponse = await _httpClient.PostAsync($"{BaseUrl}/api/menucourses/schedules", assignContent);
                    if (!assignResponse.IsSuccessStatusCode)
                        return false;
                }
            }
            else if (scheduleAssignmentId.HasValue)
            {
                // 4️⃣ Jika schedule dihapus
                var delResp = await _httpClient.DeleteAsync($"{BaseUrl}/api/menucourses/schedules/{scheduleAssignmentId.Value}");
                if (!delResp.IsSuccessStatusCode)
                    return false;
            }

            return true;
        }

        // ===============================
        // DELETE
        // ===============================
        public async Task<bool> DeleteCourseAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/menucourses/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCourseWithScheduleAsync(int courseId, int? scheduleAssignmentId, int? scheduleId)
        {
            // 1️⃣ Delete Assignment jika ada
            if (scheduleAssignmentId.HasValue)
                await _httpClient.DeleteAsync($"{BaseUrl}/api/menucourses/schedules/{scheduleAssignmentId.Value}");

            // 2️⃣ Delete Schedule jika ada
            if (scheduleId.HasValue)
                await _httpClient.DeleteAsync($"{BaseUrl}/api/schedules/{scheduleId.Value}");

            // 3️⃣ Delete Course
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/menucourses/{courseId}");
            return response.IsSuccessStatusCode;
        }



        // 🟢 Tambahan: Update schedule saja (kalau butuh manual)
        public async Task<bool> UpdateScheduleAsync(int scheduleId, DateTime newDate)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(newDate.ToString("yyyy-MM-dd HH:mm:ss")), "ScheduleDate");

            var res = await _httpClient.PutAsync($"{BaseUrl}/api/schedules/{scheduleId}", form);
            return res.IsSuccessStatusCode;
        }

        public string GetFullImageUrl(string? path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.StartsWith("http") ? path : $"{_httpClient.BaseAddress}{path.TrimStart('/')}";
        }
    }
}
