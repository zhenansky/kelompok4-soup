using AutoMapper;
using MyApp.WebAPI.Data;
using MyApp.WebAPI.DTOs;
using MyApp.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace MyApp.WebAPI.Services
{
  public class MenuCourseService : IMenuCourseService
  {
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MenuCourseService> _logger;
    private readonly IWebHostEnvironment _env;

    public MenuCourseService(ApplicationDbContext context, IMapper mapper, ILogger<MenuCourseService> logger, IWebHostEnvironment env)
    {
      _context = context;
      _mapper = mapper;
      _logger = logger;
      _env = env;
    }

    public async Task<IEnumerable<MenuCourseDto>> GetAllMenuCoursesAsync()
    {
      var menuCourses = await _context.MenuCourses
          .Include(mc => mc.Category)
          .ToListAsync();
      // Ini akan berfungsi karena DTO output (MenuCourseDto) sudah benar (menggunakan string Image)
      return _mapper.Map<IEnumerable<MenuCourseDto>>(menuCourses);
    }

    public async Task<MenuCourseDto?> GetMenuCourseByIdAsync(int id)
    {
      var menuCourse = await _context.MenuCourses
          .Include(mc => mc.Category)
          .FirstOrDefaultAsync(mc => mc.MenuCourseId == id);
      // Ini juga akan berfungsi
      return _mapper.Map<MenuCourseDto>(menuCourse);
    }

    public async Task<MenuCourseDto> CreateMenuCourseAsync(CreateMenuCourseDto createDto)
    {
      // Logika ini sudah benar
      var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == createDto.CategoryId);
      if (!categoryExists)
      {
        throw new ArgumentException($"Category dengan ID {createDto.CategoryId} tidak ditemukan.");
      }

      var menuCourse = _mapper.Map<MenuCourse>(createDto);

      if (createDto.ImageFile != null && createDto.ImageFile.Length > 0)
      {
          string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
          Directory.CreateDirectory(uploadsFolder); 
          string uniqueFileName = Guid.NewGuid().ToString() + "_" + createDto.ImageFile.FileName;
          string physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

          using (var fileStream = new FileStream(physicalPath, FileMode.Create))
          {
              await createDto.ImageFile.CopyToAsync(fileStream);
          }
          string relativePath = Path.Combine("Uploads", uniqueFileName).Replace("\\", "/");
          menuCourse.Image = relativePath;
      }

      _context.MenuCourses.Add(menuCourse);
      await _context.SaveChangesAsync();
      await _context.Entry(menuCourse).Reference(mc => mc.Category).LoadAsync();
      _logger.LogInformation("MenuCourse created with ID: {MenuCourseId}", menuCourse.MenuCourseId);
      return _mapper.Map<MenuCourseDto>(menuCourse);
    }

    public async Task<MenuCourseDto?> UpdateMenuCourseAsync(int id, UpdateMenuCourseDto updateDto)
    {
      var menuCourse = await _context.MenuCourses.FindAsync(id);
      if (menuCourse == null) return null;

      if (menuCourse.CategoryId != updateDto.CategoryId)
      {
        var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == updateDto.CategoryId);
        if (!categoryExists)
        {
          throw new ArgumentException($"Category dengan ID {updateDto.CategoryId} tidak ditemukan.");
        }
      }

      // --- TAMBAHKAN LOGIKA UPDATE GAMBAR ---
      string oldImagePath = menuCourse.Image; // 1. Simpan path gambar lama

      // Cek jika ada file baru yang di-upload
      if (updateDto.ImageFile != null && updateDto.ImageFile.Length > 0)
      {
          // 2. Simpan file baru
          string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads");
          string uniqueFileName = Guid.NewGuid().ToString() + "_" + updateDto.ImageFile.FileName;
          string physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

          using (var fileStream = new FileStream(physicalPath, FileMode.Create))
          {
              await updateDto.ImageFile.CopyToAsync(fileStream);
          }
          
          // 3. Set path baru ke database
          menuCourse.Image = Path.Combine("Uploads", uniqueFileName).Replace("\\", "/");

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

      _mapper.Map(updateDto, menuCourse);
      await _context.SaveChangesAsync();

      // ... (sisa logika Anda)
      if (menuCourse.CategoryId != updateDto.CategoryId)
      {
        await _context.Entry(menuCourse).Reference(mc => mc.Category).LoadAsync();
      }

      _logger.LogInformation("MenuCourse updated with ID: {MenuCourseId}", menuCourse.MenuCourseId);
      return _mapper.Map<MenuCourseDto>(menuCourse);
    }

    public async Task<bool> DeleteMenuCourseAsync(int id)
    {
      var menuCourse = await _context.MenuCourses.FindAsync(id);
      if (menuCourse == null) return false;

      // --- TAMBAHKAN LOGIKA DELETE GAMBAR ---
      // 1. Simpan path gambar sebelum dihapus dari DB
      string imagePath = menuCourse.Image;
      // --- SELESAI ---

      _context.MenuCourses.Remove(menuCourse);
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
      // --- SELESAI ---

      _logger.LogInformation("MenuCourse deleted with ID: {MenuCourseId}", menuCourse.MenuCourseId);
      return true;
    }
  }
}