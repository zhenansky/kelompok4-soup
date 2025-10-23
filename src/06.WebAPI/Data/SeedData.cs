using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.WebAPI.Models;

namespace MyApp.WebAPI.Data
{
  public static class SeedData
  {
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
      using var scope = serviceProvider.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
      var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

      await context.Database.MigrateAsync();

      // === SEED ROLES ===
      var roles = new[] { "Admin", "User" };
      foreach (var role in roles)
      {
        if (!await roleManager.RoleExistsAsync(role))
          await roleManager.CreateAsync(new IdentityRole<int>(role));
      }

      // === SEED USERS ===
      await SeedUserAsync(userManager, "admin@email.com", "Admin123!", "Admin", "Admin");
      await SeedUserAsync(userManager, "user1@email.com", "User123!", "User", "User 1");
      await SeedUserAsync(userManager, "user2@email.com", "User123!", "User", "User 2");

      // === SEED APP DATA ===
      await SeedAppDataAsync(context);
    }

    private static async Task SeedUserAsync(
        UserManager<User> userManager,
        string email,
        string password,
        string role,
        string name)
    {
      var user = await userManager.FindByEmailAsync(email);
      if (user == null)
      {
        user = new User
        {
          UserName = email,
          Email = email,
          Name = name,
          Status = UserStatus.Active,
          EmailConfirmed = true,
          CreatedAt = DateTime.UtcNow,
          UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
          await userManager.AddToRoleAsync(user, role);
          Console.WriteLine($"✅ Seeded user: {email} with role {role}");
        }
        else
        {
          Console.WriteLine($"⚠️ Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
      }
    }

    private static async Task SeedAppDataAsync(ApplicationDbContext context)
    {
      // === SEED CATEGORY ===
      if (!context.Categories.Any())
      {
        context.Categories.AddRange(
            new Category { Name = "Asian", Image = "Uploads/asian.svg", Description = "Masakan khas Asia", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Cold Drink", Image = "Uploads/cold_drink.svg", Description = "Minuman dingin segar", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Cookies", Image = "Uploads/cookies1.svg", Description = "Berbagai macam kue kering", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Desert", Image = "Uploads/desert.svg", Description = "Makanan penutup", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Eastern", Image = "Uploads/eastern.svg", Description = "Masakan khas Timur", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Hot Drink", Image = "Uploads/hot_drink.svg", Description = "Minuman hangat", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Junkfood", Image = "Uploads/junkfood.svg", Description = "Makanan cepat saji", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Western", Image = "Uploads/western.svg", Description = "Masakan khas Barat", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
      }

      // Ambil ID Category setelah disimpan
      var asianCategory = await context.Categories.FirstAsync(c => c.Name == "Asian");
      var coldDrinkCategory = await context.Categories.FirstAsync(c => c.Name == "Cold Drink");
      var cookiesCategory = await context.Categories.FirstAsync(c => c.Name == "Cookies");
      var dessertCategory = await context.Categories.FirstAsync(c => c.Name == "Desert");
      var westernCategory = await context.Categories.FirstAsync(c => c.Name == "Western");

      // === SEED MENU COURSE ===
      if (!context.MenuCourses.Any())
      {
        context.MenuCourses.AddRange(
            new MenuCourse
            {
              Name = "Tom Yum Thailand",
              Image = "Uploads/tomyum.svg",
              Price = 450000,
              Description = "Tom Yum Thailand",
              CategoryId = asianCategory.CategoryId,
              CreatedAt = DateTime.UtcNow,
              UpdatedAt = DateTime.UtcNow
            },
            new MenuCourse
            {
              Name = "Strawberry Float",
              Image = "Uploads/trawberry.svg",
              Price = 150000,
              Description = "Minuman strawberry float",
              CategoryId = coldDrinkCategory.CategoryId,
              CreatedAt = DateTime.UtcNow,
              UpdatedAt = DateTime.UtcNow
            },
            new MenuCourse
            {
              Name = "Chocholate Cookies",
              Image = "Uploads/cookies.svg",
              Price = 200000,
              Description = "Kue Coklat",
              CategoryId = cookiesCategory.CategoryId,
              CreatedAt = DateTime.UtcNow,
              UpdatedAt = DateTime.UtcNow
            },
            new MenuCourse
            {
              Name = "Green Tea Cheesecake",
              Image = "Uploads/greentea.svg",
              Price = 400000,
              Description = "Kue keju green tea",
              CategoryId = dessertCategory.CategoryId,
              CreatedAt = DateTime.UtcNow,
              UpdatedAt = DateTime.UtcNow
            },
             new MenuCourse
            {
              Name = "Soto Banjar Limau Kuit",
              Image = "Uploads/soto.svg",
              Price = 150000,
              Description = "Soto khas Banjar",
              CategoryId = asianCategory.CategoryId,
              CreatedAt = DateTime.UtcNow,
              UpdatedAt = DateTime.UtcNow
            },
            new MenuCourse
            {
              Name = "Italian Spaghetti Bolognese",
              Image = "Uploads/spaghetti.svg",
              Price = 450000,
              Description = "Spaghetti saus bolognese",
              CategoryId = westernCategory.CategoryId,
              CreatedAt = DateTime.UtcNow,
              UpdatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();
      }

      // Ambil ID MenuCourse
      var tomyum = await context.MenuCourses.FirstAsync(m => m.Name == "Tom Yum Thailand");
      var strawberry = await context.MenuCourses.FirstAsync(m => m.Name == "Strawberry Float");

      // === SEED SCHEDULE ===
      if (!context.Schedules.Any())
      {
        context.Schedules.AddRange(
            new Schedule { ScheduleDate = new DateTime(2022, 7, 25), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Schedule { ScheduleDate = new DateTime(2022, 7, 26), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Schedule { ScheduleDate = new DateTime(2022, 7, 27), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Schedule { ScheduleDate = new DateTime(2022, 7, 28), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Schedule { ScheduleDate = new DateTime(2022, 7, 29), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Schedule { ScheduleDate = new DateTime(2022, 7, 30), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }  
        );
        await context.SaveChangesAsync();
      }
      var schedule1 = await context.Schedules.OrderBy(s => s.ScheduleId).FirstAsync();
      var schedule2 = await context.Schedules.OrderBy(s => s.ScheduleId).Skip(1).FirstAsync();

      // === SEED MENU COURSE SCHEDULE ===
      if (!context.MenuCourseSchedules.Any())
      {
        context.MenuCourseSchedules.AddRange(
            new MenuCourseSchedule
            {
              MenuCourseId = tomyum!.MenuCourseId,
              ScheduleId = schedule1.ScheduleId,
              AvailableSlot = 10,
              Status = MSStatus.Active,
              CreatedAt = DateTime.UtcNow,
              UpdatedAt = DateTime.UtcNow
            },
            new MenuCourseSchedule
            {
              MenuCourseId = strawberry.MenuCourseId,
              ScheduleId = schedule2.ScheduleId,
              AvailableSlot = 8,
              Status = MSStatus.Active,
              CreatedAt = DateTime.UtcNow,
              UpdatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();
      }

      // === SEED PAYMENT METHODS ===
      if (!context.PaymentMethods.Any())
      {
        context.PaymentMethods.AddRange(
            new PaymentMethod { Name = "BCA", Logo = "bca.png", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new PaymentMethod { Name = "GoPay", Logo = "gopay.png", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
      }
    }
  }
}
