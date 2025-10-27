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
           new Category { Name = "Asian", Image = "Uploads/asian.svg", Description = "Jelajahi cita rasa otentik dari berbagai penjuru Asia, dari hidangan tumis hingga berkuah kaya rempah.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Cold Drink", Image = "Uploads/cold_drink.svg", Description = "Segarkan harimu dengan pilihan minuman dingin kami, mulai dari jus buah murni hingga soda menyegarkan.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Cookies", Image = "Uploads/cookies1.svg", Description = "Temukan berbagai macam kue kering renyah dan lezat, sempurna untuk camilan atau teman minum teh.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Desert", Image = "Uploads/desert.svg", Description = "Tutup hidangan Anda dengan sempurna. Pilihan hidangan penutup manis yang memanjakan lidah.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Eastern", Image = "Uploads/eastern.svg", Description = "Nikmati kekayaan rasa dari hidangan khas Timur Tengah, penuh dengan bumbu dan aroma yang menggugah selera.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Hot Drink", Image = "Uploads/hot_drink.svg", Description = "Hangatkan tubuh dan tenangkan pikiran dengan koleksi minuman hangat kami, termasuk kopi premium dan teh herbal.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Junkfood", Image = "Uploads/junkfood.svg", Description = "Pilihan cepat, praktis, dan lezat untuk Anda yang sedang bepergian. Burger, kentang goreng, dan lainnya.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Name = "Western", Image = "Uploads/western.svg", Description = "Cicipi hidangan klasik dari Barat, mulai dari steak yang juicy, pasta al dente, hingga salad segar.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
      }

      // Ambil ID Category setelah disimpan
    var asianCategory = await context.Categories.FirstAsync(c => c.Name == "Asian");
    var coldDrinkCategory = await context.Categories.FirstAsync(c => c.Name == "Cold Drink");
    var cookiesCategory = await context.Categories.FirstAsync(c => c.Name == "Cookies");
    var desertCategory = await context.Categories.FirstAsync(c => c.Name == "Desert");
    var easternCategory = await context.Categories.FirstAsync(c => c.Name == "Eastern");
    var hotDrinkCategory = await context.Categories.FirstAsync(c => c.Name == "Hot Drink");
    var junkfoodCategory = await context.Categories.FirstAsync(c => c.Name == "Junkfood");
    var westernCategory = await context.Categories.FirstAsync(c => c.Name == "Western");

      // === SEED MENU COURSE ===
      if (!context.MenuCourses.Any())
      {
        context.MenuCourses.AddRange(
           // --- 6 Items Sesuai Gambar ---
            new MenuCourse { Name = "Tom Yum Thailand", Image = "Uploads/tomyum.svg", Price = 450000, Description = "Sup asam pedas khas Thailand dengan isian seafood segar, jamur, dan bumbu rempah otentik.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Strawberry Float", Image = "Uploads/trawberry.svg", Price = 150000, Description = "Minuman float lembut dibuat dari es krim vanilla dan buah strawberry asli.", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Chocholate Cookies", Image = "Uploads/cookies.svg", Price = 200000, Description = "Kue kering renyah yang kaya akan potongan cokelat premium.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Green Tea Cheesecake", Image = "Uploads/greentea.svg", Price = 400000, Description = "Cheesecake lembut dengan rasa matcha premium dan alas biskuit renyah.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Soto Banjar Limau Kuit", Image = "Uploads/soto.svg", Price = 150000, Description = "Soto khas Banjar dengan suwiran ayam, perkedel, dan aroma segar limau kuit.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Italian Spaghetti Bolognese", Image = "Uploads/spaghetti.svg", Price = 450000, Description = "Pasta spaghetti al dente disajikan dengan saus bolognese klasik dari daging dan tomat.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Asian (Lanjutan) ---
            new MenuCourse { Name = "Nasi Goreng Spesial", Image = "Uploads/nasigoreng.svg", Price = 55000, Description = "Nasi goreng dengan campuran telur, ayam, udang, dan sayuran, disajikan dengan acar.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Ramen Miso", Image = "Uploads/ramen.svg", Price = 75000, Description = "Mi ramen dengan kuah miso gurih, topping chashu ayam, telur rebus, dan nori.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Pad Thai", Image = "Uploads/padthai.svg", Price = 65000, Description = "Kwetiau goreng khas Thailand dengan saus asam manis, tahu, udang, dan taburan kacang.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Sate Ayam Madura", Image = "Uploads/sate.svg", Price = 45000, Description = "10 tusuk sate ayam empuk dengan bumbu kacang khas Madura, disajikan dengan lontong.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Sushi Platter", Image = "Uploads/sushi.svg", Price = 120000, Description = "Satu set sushi berisi salmon roll, california roll, dan tuna nigiri.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Cold Drink (Lanjutan) ---
            new MenuCourse { Name = "Es Kopi Susu Gula Aren", Image = "Uploads/eskopi.svg", Price = 25000, Description = "Perpaduan espresso, susu segar, dan manisnya gula aren asli.", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Jus Mangga Segar", Image = "Uploads/jusmangga.svg", Price = 30000, Description = "Jus dari buah mangga Harum Manis pilihan tanpa tambahan gula.", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Lemon Squash", Image = "Uploads/lemonsquash.svg", Price = 28000, Description = "Minuman soda menyegarkan dengan perasan jeruk lemon asli dan daun mint.", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Thai Tea Original", Image = "Uploads/thaitea.svg", Price = 22000, Description = "Teh hitam khas Thailand yang dicampur dengan susu kental manis.", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Es Teh Leci", Image = "Uploads/estehleci.svg", Price = 26000, Description = "Es teh manis dengan tambahan sirup dan buah leci yang menyegarkan.", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Cookies (Lanjutan) ---
            new MenuCourse { Name = "Nastar Premium", Image = "Uploads/nastar.svg", Price = 150000, Description = "Kue kering isi selai nanas homemade dengan adonan mentega Wijsman yang lumer.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Kaasstengels", Image = "Uploads/kaasstengels.svg", Price = 165000, Description = "Kue keju batangan yang renyah dan gurih menggunakan keju Edam asli.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Putri Salju", Image = "Uploads/putrisalju.svg", Price = 135000, Description = "Kue berbentuk bulan sabit dengan taburan gula halus seperti salju.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Lidah Kucing Rainbow", Image = "Uploads/lidahkucing.svg", Price = 110000, Description = "Kue kering tipis dan renyah dengan sentuhan warna-warni yang menarik.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Oatmeal Raisin Cookies", Image = "Uploads/oatmeal.svg", Price = 125000, Description = "Kue sehat dan lezat yang terbuat dari oatmeal dan kismis manis.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Dessert (Lanjutan) ---
            new MenuCourse { Name = "Lava Cake Cokelat", Image = "Uploads/lavacake.svg", Price = 55000, Description = "Kue cokelat hangat dengan lelehan cokelat di dalamnya, disajikan dengan es krim vanilla.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Panna Cotta", Image = "Uploads/pannacotta.svg", Price = 45000, Description = "Puding krim lembut khas Italia dengan saus strawberry segar.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Tiramisu", Image = "Uploads/tiramisu.svg", Price = 65000, Description = "Dessert kopi khas Italia dengan lapisan ladyfingers, keju mascarpone, dan taburan bubuk kakao.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Es Pisang Ijo", Image = "Uploads/pisangijo.svg", Price = 35000, Description = "Pisang berbalut adonan hijau, disajikan dengan bubur sumsum, sirup, dan es serut.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Brownies Panggang", Image = "Uploads/brownies.svg", Price = 40000, Description = "Potongan brownies cokelat pekat dengan topping kacang almond.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Eastern ---
            new MenuCourse { Name = "Kebab Daging Sapi", Image = "Uploads/kebab.svg", Price = 45000, Description = "Daging sapi panggang berbumbu yang disajikan dalam roti pita hangat dengan sayuran segar.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Nasi Biryani Ayam", Image = "Uploads/biryani.svg", Price = 70000, Description = "Nasi basmati kaya rempah yang dimasak dengan potongan ayam empuk.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Hummus Platter", Image = "Uploads/hummus.svg", Price = 50000, Description = "Pasta kacang arab lembut disajikan dengan minyak zaitun dan roti pita.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Chicken Shawarma", Image = "Uploads/shawarma.svg", Price = 48000, Description = "Gulungan roti berisi irisan ayam panggang, saus bawang putih, dan acar.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Falafel", Image = "Uploads/falafel.svg", Price = 40000, Description = "Bola-bola goreng yang terbuat dari kacang arab giling, disajikan dengan saus tahini.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Umm Ali", Image = "Uploads/ummali.svg", Price = 55000, Description = "Puding roti khas Mesir yang kaya rasa, disajikan hangat dengan taburan kacang.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // --- Hot Drink ---
            new MenuCourse { Name = "Cappuccino", Image = "Uploads/cappuccino.svg", Price = 35000, Description = "Espresso dengan steamed milk dan busa susu tebal di atasnya.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Teh Tarik", Image = "Uploads/tehtarik.svg", Price = 20000, Description = "Teh hitam pekat yang dicampur dengan susu dan 'ditarik' untuk menciptakan busa.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Wedang Jahe", Image = "Uploads/wedangjahe.svg", Price = 18000, Description = "Minuman jahe tradisional yang menghangatkan tubuh, dimaniskan dengan gula aren.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Hot Chocolate", Image = "Uploads/hotchocolate.svg", Price = 32000, Description = "Cokelat premium yang dilelehkan dengan susu hangat, menghasilkan rasa yang kaya.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Matcha Latte", Image = "Uploads/matchalatte.svg", Price = 38000, Description = "Bubuk matcha kualitas terbaik yang dikocok dengan steamed milk lembut.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Kopi Tubruk", Image = "Uploads/kopitubruk.svg", Price = 15000, Description = "Kopi hitam khas Indonesia yang diseduh langsung dari bubuk kopi halus.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Junkfood ---
            new MenuCourse { Name = "Beef Burger Deluxe", Image = "Uploads/burger.svg", Price = 65000, Description = "Burger dengan patty daging sapi tebal, keju cheddar, selada, tomat, dan saus spesial.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Kentang Goreng", Image = "Uploads/fries.svg", Price = 25000, Description = "Kentang goreng renyah yang ditaburi garam, disajikan dengan saus tomat.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Pizza Pepperoni", Image = "Uploads/pizza.svg", Price = 95000, Description = "Pizza tipis dengan topping saus tomat, keju mozzarella, dan pepperoni berlimpah.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Ayam Goreng Crispy", Image = "Uploads/friedchicken.svg", Price = 30000, Description = "Satu potong ayam goreng dengan balutan tepung renyah dan bumbu rahasia.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Hotdog", Image = "Uploads/hotdog.svg", Price = 40000, Description = "Roti bun lembut dengan sosis sapi panggang, saus mustard, dan saus tomat.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Onion Rings", Image = "Uploads/onionrings.svg", Price = 32000, Description = "Cincin bawang bombay yang digoreng renyah dengan adonan spesial.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // --- Western (Lanjutan) ---
            new MenuCourse { Name = "Sirloin Steak", Image = "Uploads/steak.svg", Price = 185000, Description = "200gr daging sirloin impor disajikan dengan saus jamur, kentang, dan sayuran.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Chicken Cordon Bleu", Image = "Uploads/cordonbleu.svg", Price = 95000, Description = "Dada ayam gulung berisi daging asap dan keju leleh, digoreng hingga keemasan.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Fish and Chips", Image = "Uploads/fishnchips.svg", Price = 75000, Description = "Ikan dori goreng tepung renyah disajikan dengan kentang goreng dan saus tartar.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Caesar Salad", Image = "Uploads/caesarsalad.svg", Price = 60000, Description = "Salad dengan daun selada romaine, crouton, keju parmesan, dan saus caesar.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Lasagna al Forno", Image = "Uploads/lasagna.svg", Price = 90000, Description = "Lapisan pasta dengan saus daging, saus bechamel, dan keju mozzarella panggang.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
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
           new PaymentMethod { Name = "Gopay", Logo = "gopay.svg", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new PaymentMethod { Name = "OVO", Logo = "ovo.svg", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new PaymentMethod { Name = "DANA", Logo = "dana.svg", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new PaymentMethod { Name = "Mandiri", Logo = "mandiri.svg", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new PaymentMethod { Name = "BCA", Logo = "bca.svg", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new PaymentMethod { Name = "BNI", Logo = "bni.svg", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
      }
    }
  }
}
