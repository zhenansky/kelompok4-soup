using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.WebAPI.Data;
using MyApp.WebAPI.Models;
using MyApp.WebAPI.DTOs;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace MyApp.WebAPI.Controllers
{
    [Route("api/payment-methods")]
    [ApiController]
    public class PaymentMethodController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly string _uploadPath;

        public PaymentMethodController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            _uploadPath = Path.Combine(_env.WebRootPath, "payment-logos");

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _context.PaymentMethods.ToListAsync();
                return Ok(new { success = true, message = "Payment methods retrieved successfully.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving data.", error = ex.Message });
            }
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var payment = await _context.PaymentMethods.FindAsync(id);
                if (payment == null)
                    return NotFound(new { success = false, message = "Payment method not found." });

                return Ok(new { success = true, message = "Payment method retrieved successfully.", data = payment });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving payment method.", error = ex.Message });
            }
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PaymentMethodCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data." });

                if (dto.LogoFile == null || dto.LogoFile.Length == 0)
                    return BadRequest(new { success = false, message = "Logo file is required." });

                bool exists = await _context.PaymentMethods.AnyAsync(p => p.Name.ToLower() == dto.Name.ToLower());
                if (exists)
                    return Conflict(new { success = false, message = $"Payment method '{dto.Name}' already exists." });

                string uniqueFileName = await SaveFile(dto.LogoFile);

                var payment = new PaymentMethod
                {
                    Name = dto.Name,
                    Logo = uniqueFileName,
                    Status = dto.Status,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.PaymentMethods.Add(payment);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Payment method created successfully.", data = payment });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error creating payment method.", error = ex.Message });
            }
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] PaymentMethodUpdateDto dto)
        {
            try
            {
                var payment = await _context.PaymentMethods.FindAsync(id);
                if (payment == null)
                    return NotFound(new { success = false, message = "Payment method not found." });

                bool nameExists = await _context.PaymentMethods
                    .AnyAsync(p => p.Name.ToLower() == dto.Name.ToLower() && p.PaymentMethodId != id);

                if (nameExists)
                    return Conflict(new { success = false, message = $"Payment method '{dto.Name}' already exists." });

                if (dto.LogoFile != null && dto.LogoFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(payment.Logo))
                    {
                        DeleteFile(payment.Logo);
                    }
                    payment.Logo = await SaveFile(dto.LogoFile);
                }

                payment.Name = dto.Name;
                payment.Status = dto.Status;
                payment.UpdatedAt = DateTime.Now;

                _context.PaymentMethods.Update(payment);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Payment method updated successfully.", data = payment });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating payment method.", error = ex.Message });
            }
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var payment = await _context.PaymentMethods.FindAsync(id);
                if (payment == null)
                    return NotFound(new { success = false, message = "Payment method not found." });

                if (!string.IsNullOrEmpty(payment.Logo))
                {
                    DeleteFile(payment.Logo);
                }

                _context.PaymentMethods.Remove(payment);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Payment method deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting payment method.", error = ex.Message });
            }
        }

        // --- PRIVATE HELPER METHODS ---

        private async Task<string> SaveFile(IFormFile file)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            string filePath = Path.Combine(_uploadPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return uniqueFileName;
        }

        private void DeleteFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_uploadPath, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file {fileName}: {ex.Message}");
            }
        }
    }
}