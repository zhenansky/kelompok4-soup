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

      var courses = wrapper.Data.Select(dto => new CourseModel
      {
        Id = dto.Id,
        Name = dto.Name,
        CategoryId = dto.CategoryId,
        CategoryName = dto.CategoryName,
        ImageUrl = string.IsNullOrEmpty(dto.Image) ? null : $"{BaseUrl}/{dto.Image.TrimStart('/')}",
        Price = dto.Price,
        Description = dto.Description ?? "-"
      }).ToList();

      return courses;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
      var wrapper = await _httpClient.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>($"{BaseUrl}/api/categories");
      return wrapper?.Data ?? new List<CategoryDto>();
    }

    // ===============================
    // CREATE
    // ===============================
    public async Task<int?> AddCourseAsync(CreateCourseDto dto, IBrowserFile? file)
    {
      using var content = new MultipartFormDataContent();

      content.Add(new StringContent(dto.Name ?? ""), "Name");
      content.Add(new StringContent(dto.Description ?? ""), "Description");
      content.Add(new StringContent(dto.Price.ToString()), "Price");
      content.Add(new StringContent(dto.CategoryId.ToString()), "CategoryId");

      if (file != null)
      {
        var stream = file.OpenReadStream(8 * 1024 * 1024);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "ImageFile", file.Name);
      }

      var response = await _httpClient.PostAsync($"{BaseUrl}/api/menucourses", content);
      if (!response.IsSuccessStatusCode)
        return null;

      var created = await response.Content.ReadFromJsonAsync<ApiResponse<MenuCourseDto>>();
      return created?.Data?.Id;
    }

    // ===============================
    // UPDATE
    // ===============================
    public async Task<bool> UpdateCourseAsync(int id, CreateCourseDto dto, IBrowserFile? file)
    {
      using var content = new MultipartFormDataContent();
      content.Add(new StringContent(dto.Name ?? ""), "Name");
      content.Add(new StringContent(dto.Description ?? ""), "Description");
      content.Add(new StringContent(dto.Price.ToString()), "Price");
      content.Add(new StringContent(dto.CategoryId.ToString()), "CategoryId");

      if (file != null)
      {
        var stream = file.OpenReadStream(8 * 1024 * 1024);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "ImageFile", file.Name);
      }

      var updateResponse = await _httpClient.PutAsync($"{BaseUrl}/api/menucourses/{id}", content);
      return updateResponse.IsSuccessStatusCode;
    }

    // ===============================
    // DELETE
    // ===============================
    public async Task<bool> DeleteCourseAsync(int id)
    {
      var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/menucourses/{id}");
      return response.IsSuccessStatusCode;
    }

    // ===============================
    // UTIL
    // ===============================
    public string GetFullImageUrl(string? path)
    {
      if (string.IsNullOrEmpty(path)) return string.Empty;
      return path.StartsWith("http") ? path : $"{_httpClient.BaseAddress}{path.TrimStart('/')}";
    }
  }
}
