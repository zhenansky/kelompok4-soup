using AutoMapper;
using MyApp.WebAPI.Data;
using MyApp.WebAPI.DTOs;
using MyApp.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace MyApp.WebAPI.Services
{
  public class CategoryService : ICategoryService
  {
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;
    private readonly IWebHostEnvironment _env;

    public CategoryService(ApplicationDbContext context, IMapper mapper, ILogger<CategoryService> logger, IWebHostEnvironment env)
    {
      _context = context;
      _mapper = mapper;
      _logger = logger;
      _env = env;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
      var categories = await _context.Categories
          .Include(c => c.MenuCourses)
          .ToListAsync();

      return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
      var category = await _context.Categories
          .Include(c => c.MenuCourses)
          .FirstOrDefaultAsync(c => c.CategoryId == id);

      return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
      var category = _mapper.Map<Category>(createCategoryDto);

      // Logika ini sudah benar
      if (createCategoryDto.ImageFile != null && createCategoryDto.ImageFile.Length > 0)
      {
          string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
          Directory.CreateDirectory(uploadsFolder); 
          
          string uniqueFileName = Guid.NewGuid().ToString() + "_" + createCategoryDto.ImageFile.FileName;
          string physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

          using (var fileStream = new FileStream(physicalPath, FileMode.Create))
          {
              await createCategoryDto.ImageFile.CopyToAsync(fileStream);
          }
          
          string relativePath = Path.Combine("Uploads", uniqueFileName).Replace("\\", "/");
          category.Image = relativePath;
      }

      _context.Categories.Add(category);
      await _context.SaveChangesAsync();

      _logger.LogInformation("Category created: {CategoryName} with ID: {CategoryId}",
          category.Name, category.CategoryId);

      var categoryDto = _mapper.Map<CategoryDto>(category);
      categoryDto.MenuCourseCount = 0; 

      return categoryDto;
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
    {
      var category = await _context.Categories
          .Include(c => c.MenuCourses)
          .FirstOrDefaultAsync(c => c.CategoryId == id);

      if (category == null)
      {
        return null;
      }

      // --- TAMBAHKAN LOGIKA UPDATE GAMBAR ---
      string oldImagePath = category.Image; // 1. Simpan path gambar lama

      // Cek jika ada file baru yang di-upload
      if (updateCategoryDto.ImageFile != null && updateCategoryDto.ImageFile.Length > 0)
      {
          // 2. Simpan file baru (kode sama seperti di CreateAsync)
          string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
          string uniqueFileName = Guid.NewGuid().ToString() + "_" + updateCategoryDto.ImageFile.FileName;
          string physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

          using (var fileStream = new FileStream(physicalPath, FileMode.Create))
          {
              await updateCategoryDto.ImageFile.CopyToAsync(fileStream);
          }
          
          // 3. Set path baru ke database
          category.Image = Path.Combine("Uploads", uniqueFileName).Replace("\\", "/");

          // 4. Hapus file lama (jika ada)
          if (!string.IsNullOrEmpty(oldImagePath))
          {
              string oldPhysicalPath = Path.Combine(_env.WebRootPath, oldImagePath);
              if (File.Exists(oldPhysicalPath))
              {
                  File.Delete(oldPhysicalPath);
              }
          }
      }
      // --- SELESAI ---

      // Map sisa properti (seperti Name)
      _mapper.Map(updateCategoryDto, category);

      await _context.SaveChangesAsync();

      _logger.LogInformation("Category updated: {CategoryId}", id);

      return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
      var category = await _context.Categories.FindAsync(id);
      if (category == null)
      {
        return false;
      }

      string imagePath = category.Image;

      _context.Categories.Remove(category);
      await _context.SaveChangesAsync();

      // --- TAMBAHKAN LOGIKA DELETE GAMBAR ---
      // 2. Hapus file fisik dari server
      if (!string.IsNullOrEmpty(imagePath))
      {
          string physicalPath = Path.Combine(_env.WebRootPath, imagePath);
          if (File.Exists(physicalPath))
          {
              File.Delete(physicalPath);
          }
      }

      _logger.LogInformation("Category deleted: {CategoryId}", id);

      return true;
    }

    public async Task<bool> CategoryExistsAsync(int id)
    {
      return await _context.Categories.AnyAsync(c => c.CategoryId == id);
    }
  }
}