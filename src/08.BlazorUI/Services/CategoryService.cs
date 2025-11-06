using System.Net.Http.Headers;
using System.Net.Http.Json;
using MyApp.BlazorUI.Models;
using MyApp.BlazorUI.DTOs;

namespace MyApp.BlazorUI.Services
{
  public class CategoryService
  {
    private readonly HttpClient _http;

    public CategoryService(HttpClient http)
    {
      _http = http;
    }

    // ✅ GET ALL
    public async Task<List<CategoryDto>> GetAllAsync()
    {
      try
      {
        // --- Cek apakah backend kirim data langsung atau pakai wrapper ---
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>("api/categories");

        if (response?.Data != null)
          return response.Data;

        // fallback: kalau backend return langsung list
        var direct = await _http.GetFromJsonAsync<List<CategoryDto>>("api/categories");
        return direct ?? new List<CategoryDto>();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ [GetAllAsync] {ex.Message}");
        return new List<CategoryDto>();
      }
    }

    // ✅ ADD
    public async Task<CategoryDto?> AddAsync(CreateCategoryDto model)
    {
      try
      {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(model.Name ?? string.Empty), "Name");
        if (!string.IsNullOrWhiteSpace(model.Description))
          content.Add(new StringContent(model.Description), "Description");

        if (model.ImageFile != null)
        {
          var stream = model.ImageFile.OpenReadStream(maxAllowedSize: 8_000_000);
          var fileContent = new StreamContent(stream);
          fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
          // ⬇️ ganti key sesuai backend-mu (Image / ImageFile)
          content.Add(fileContent, "ImageFile", model.ImageFile.Name);
        }

        var response = await _http.PostAsync("api/categories", content);
        if (!response.IsSuccessStatusCode)
        {
          Console.WriteLine($"❌ [AddAsync] Failed: {response.StatusCode}");
          return null;
        }

        // Coba baca wrapper atau langsung object
        var apiResp = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        return apiResp?.Data ?? await response.Content.ReadFromJsonAsync<CategoryDto>();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ [AddAsync] {ex.Message}");
        return null;
      }
    }

    // ✅ UPDATE
    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto model)
    {
      try
      {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(model.Name ?? string.Empty), "Name");
        if (!string.IsNullOrWhiteSpace(model.Description))
          content.Add(new StringContent(model.Description), "Description");

        if (model.ImageFile != null)
        {
          var stream = model.ImageFile.OpenReadStream(maxAllowedSize: 8_000_000);
          var fileContent = new StreamContent(stream);
          fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
          content.Add(fileContent, "ImageFile", model.ImageFile.Name);
        }

        var response = await _http.PutAsync($"api/categories/{id}", content);
        if (!response.IsSuccessStatusCode)
        {
          Console.WriteLine($"❌ [UpdateAsync] Failed: {response.StatusCode}");
          return null;
        }

        var apiResp = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        return apiResp?.Data ?? await response.Content.ReadFromJsonAsync<CategoryDto>();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ [UpdateAsync] {ex.Message}");
        return null;
      }
    }

    // ✅ DELETE
    public async Task<bool> DeleteAsync(int id)
    {
      try
      {
        var response = await _http.DeleteAsync($"api/categories/{id}");
        return response.IsSuccessStatusCode;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ [DeleteAsync] {ex.Message}");
        return false;
      }
    }

    public string GetFullImageUrl(string? path)
    {
      if (string.IsNullOrEmpty(path)) return string.Empty;
      return path.StartsWith("http") ? path : $"{_http.BaseAddress}{path.TrimStart('/')}";
    }
  }
}
