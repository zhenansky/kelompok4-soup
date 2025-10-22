using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MyApp.WebAPI.Data;
using MyApp.WebAPI.DTOs;
using MyApp.WebAPI.Models;
using MyApp.WebAPI.Exceptions;
using MyApp.WebAPI.Extensions;
using MyApp.WebAPI.Services.Interfaces;
using System.Security.Claims;

namespace MyApp.WebAPI.Services
{
  public class InvoiceService : IInvoiceService
  {
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public InvoiceService(ApplicationDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
      _context = context;
      _mapper = mapper;
      _httpContextAccessor = httpContextAccessor;
    }

    private int GetCurrentUserId()
    {
      var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      if (userIdClaim == null) throw new UnauthorizedAccessException("User not authenticated.");
      return int.Parse(userIdClaim);
    }

    private bool IsAdmin()
    {
      return _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
    }

    public async Task<PagedResult<InvoiceDTO>> GetAllInvoicesAsync(int pageNumber = 1, int pageSize = 10)
    {
      if (!IsAdmin())
        throw new ForbiddenException("Only administrators can access all invoices.");

      var invoices = _context.Invoices
          .Include(i => i.User) // ⬅️ tambahan penting
          .AsNoTracking()
          .ProjectTo<InvoiceDTO>(_mapper.ConfigurationProvider);

      return await invoices.ToPagedResultAsync(pageNumber, pageSize);
    }

    public async Task<IEnumerable<InvoiceDTO>> GetUserInvoicesAsync(int userId)
    {
      var currentUserId = GetCurrentUserId();
      var isAdmin = IsAdmin();

      if (!isAdmin && currentUserId != userId)
        throw new ForbiddenException("You can only access your own invoices.");


      var user = await _context.Users.AnyAsync(u => u.Id == userId);
      if (!user)
        throw new NotFoundException($"User with ID {userId} not found");

      var invoices = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.UserIdRef == userId)
                .ProjectTo<InvoiceDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

      return invoices;
    }

    public async Task<DetailInvoiceDTO?> GetInvoiceDetailAsync(int id)
    {
      var currentUserId = GetCurrentUserId();
      var isAdmin = IsAdmin();

      var invoice = await _context.Invoices
              .Include(i => i.User)
              .Where(i => i.InvoiceId == id)
              .ProjectTo<DetailInvoiceDTO>(_mapper.ConfigurationProvider)
              .FirstOrDefaultAsync();


      if (invoice == null)
        throw new NotFoundException($"Invoice with ID {id} not found");

      if (!isAdmin && invoice.UserIdRef != currentUserId)
        throw new ForbiddenException("You can only access your own invoice details.");


      return invoice;
    }

    public async Task<object> CreateInvoiceAsync(CreateInvoiceDTO createInvoiceDTO)
    {
      var currentUserId = GetCurrentUserId();

      // check user not found
      var user = await _context.Users.FindAsync(currentUserId);
      if (user == null) throw new NotFoundException("User not found.");

      // check not select any course
      if (createInvoiceDTO.MSId == null || createInvoiceDTO.MSId.Count == 0)
        throw new ValidationException("No course selected.");

      // check duplicate course
      var duplicateIds = createInvoiceDTO.MSId
                    .GroupBy(id => id)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
      if (duplicateIds.Count != 0)
        throw new ValidationException("Duplicate courses detected.", new { duplicateIds });

      using var transaction = await _context.Database.BeginTransactionAsync();

      try
      {
        var schedules = await _context.MenuCourseSchedules
                    .Include(ms => ms.MenuCourse)
                    .Where(ms => createInvoiceDTO.MSId.Contains(ms.MSId))
                    .ToListAsync();

        // check if course not in database
        var missingIds = createInvoiceDTO.MSId.Except(schedules.Select(s => s.MSId)).ToList();
        if (missingIds.Count != 0)
          throw new NotFoundException("Some courses were not found.", new { missingIds });

        // check if course already purchased
        var purchasedIds = await _context.MyClasses
            .Where(mc => mc.UserIdRef == currentUserId && createInvoiceDTO.MSId.Contains(mc.MSId))
            .Select(mc => mc.MSId)
            .ToListAsync();
        if (purchasedIds.Count != 0)
        {
          var courses = schedules
              .Where(s => purchasedIds.Contains(s.MSId))
              .Select(s => new
              {
                MSId = s.MSId,
                CourseName = s.MenuCourse.Name
              })
              .ToList();
          throw new ValidationException("You already purchased these courses.", new { courses });
        }

        // check if course is full
        foreach (var schedule in schedules)
        {
          if (schedule.AvailableSlot <= 0 || schedule.Status == MSStatus.Inactive)
            throw new ValidationException($"Schedule {schedule.MSId} ({schedule.MenuCourse.Name}) is full.");
        }

        var totalPrice = schedules.Sum(s => s.MenuCourse.Price);
        var totalCourse = schedules.Count;

        // invoice number
        var lastInvoice = await _context.Invoices
            .OrderByDescending(i => i.InvoiceId)
            .FirstOrDefaultAsync();
        int nextNumber = 1;
        if (lastInvoice != null && lastInvoice.NoInvoice.Length >= 8)
        {
          var numberPart = lastInvoice.NoInvoice.Substring(3);
          if (int.TryParse(numberPart, out int lastNumber))
            nextNumber = lastNumber + 1;
        }
        var invoiceNumber = $"SOU{nextNumber:D5}";

        var invoice = new Invoice
        {
          NoInvoice = invoiceNumber,
          Date = DateTime.UtcNow,
          TotalCourse = totalCourse,
          TotalPrice = totalPrice,
          UserIdRef = currentUserId
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        foreach (var schedule in schedules)
        {
          schedule.AvailableSlot -= 1;
          if (schedule.AvailableSlot <= 0) schedule.Status = MSStatus.Inactive;

          var invoiceMenuCourse = new InvoiceMenuCourse
          {
            InvoiceId = invoice.InvoiceId,
            MSId = schedule.MSId
          };
          _context.InvoiceMenuCourses.Add(invoiceMenuCourse);

          var myClass = new MyClass
          {
            UserIdRef = currentUserId,
            MSId = schedule.MSId
          };
          _context.MyClasses.Add(myClass);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new
        {
          invoiceId = invoice.InvoiceId,
          noInvoice = invoice.NoInvoice,
          date = invoice.Date,
          totalCourse = invoice.TotalCourse,
          totalPrice = invoice.TotalPrice,
          userId = invoice.UserIdRef
        };
      }
      catch
      {
        await transaction.RollbackAsync();
        throw;
      }
    }
  }
}