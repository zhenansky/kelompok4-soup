using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.WebAPI.Data;
using MyApp.WebAPI.Models;
using MyApp.WebAPI.DTOs;

namespace MyApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
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

        // ✅ GET BY ID
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

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentMethodCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data." });

                // 🔍 Cek duplikat nama
                bool exists = await _context.PaymentMethods.AnyAsync(p => p.Name.ToLower() == dto.Name.ToLower());
                if (exists)
                    return Conflict(new { success = false, message = $"Payment method '{dto.Name}' already exists." });

                var payment = new PaymentMethod
                {
                    Name = dto.Name,
                    Logo = dto.Logo,
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

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentMethodUpdateDto dto)
        {
            try
            {
                var payment = await _context.PaymentMethods.FindAsync(id);
                if (payment == null)
                    return NotFound(new { success = false, message = "Payment method not found." });

                // 🔍 Cek duplikat nama (kecuali dirinya sendiri)
                bool nameExists = await _context.PaymentMethods
                    .AnyAsync(p => p.Name.ToLower() == dto.Name.ToLower() && p.PaymentMethodId != id);

                if (nameExists)
                    return Conflict(new { success = false, message = $"Payment method '{dto.Name}' already exists." });

                payment.Name = dto.Name;
                payment.Logo = dto.Logo;
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

        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var payment = await _context.PaymentMethods.FindAsync(id);
                if (payment == null)
                    return NotFound(new { success = false, message = "Payment method not found." });

                _context.PaymentMethods.Remove(payment);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Payment method deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting payment method.", error = ex.Message });
            }
        }
    }
}
