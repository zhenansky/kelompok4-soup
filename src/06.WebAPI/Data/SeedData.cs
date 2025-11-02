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
           new Category {
        Name = "Asian",
        Image = "Uploads/asian.svg",
        Description = "Embark on a culinary journey across the diverse continent of Asia. Explore authentic flavors from the bustling street food stalls of Thailand to the refined noodle houses of Japan, featuring everything from savory stir-fries to rich, aromatic, and spicy curries.\n\nThis culinary tradition is built on a foundation of balance, masterfully blending sweet, sour, salty, bitter, and umami. Staples like rice and noodles are paired with powerful aromatics such as ginger, garlic, and soy, creating a complex yet harmonious flavor profile in every dish.",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Category {
        Name = "Cold Drink",
        Image = "Uploads/cold_drink.svg",
        Description = "Quench your thirst and refresh your day with our extensive selection of cold beverages. Choose from pure, freshly squeezed fruit juices, iced coffees, creamy milkshakes, or sparkling sodas to perfectly complement your meal.\n\nBeyond simple refreshment, this category is constantly evolving. We embrace global trends, offering everything from functional beverages packed with vitamins to artisanal craft sodas and sophisticated, low-sugar iced teas that cater to a modern, health-conscious palate.",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Category {
        Name = "Cookies",
        Image = "Uploads/cookies1.svg",
        Description = "Indulge your sweet tooth with our delightful assortment of freshly baked cookies. Discover everything from classic chocolate chip to crispy, buttery creations, each one perfect for a comforting snack or as a companion to your afternoon tea or coffee.\n\nMore than just a treat, cookies are a universal symbol of comfort and celebration. They evoke feelings of home and are perfect for sharing, representing a simple, heartfelt gesture that brings joy to any occasion, from holiday gatherings to quiet afternoons.",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Category {
        Name = "Desert", // (Typo: Sebaiknya 'Dessert')
        Image = "Uploads/desert.svg",
        Description = "End your meal on a high note with our exquisite selection of desserts. From rich, decadent chocolate lava cakes to light and creamy panna cottas, our sweet treats are crafted to provide the perfect, indulgent conclusion to your dining experience.\n\nA great dessert is a multi-sensory experience, focusing on the interplay of texture, temperature, and presentation. It serves as the final flourish, providing a memorable and satisfying close that balances the entire meal and leaves a lasting impression.",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Category {
        Name = "Eastern",
        Image = "Uploads/eastern.svg",
        Description = "Savor the rich and aromatic flavors of the Middle East. This category features dishes defined by exotic spices, grilled meats, and savory dips, offering a truly appetizing experience that transports you to an Eastern bazaar.\n\nCentral to this cuisine is the concept of 'meze,' a vibrant collection of small dishes designed for sharing, which embodies hospitality and community. Expect signature spices like sumac, za'atar, and cumin, which provide an unforgettable depth of flavor.",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Category {
        Name = "Hot Drink",
        Image = "Uploads/hot_drink.svg",
        Description = "Warm your body and soothe your soul with our comforting collection of hot beverages. Whether you need a robust, premium-roast coffee to start your day or a calming herbal tea to unwind, we have the perfect hot drink for any mood.\n\nThese beverages are more than just a source of warmth; they are a global ritual. They foster social connections, fuel conversations, and provide a cherished moment for personal reflection and mindfulness in a busy world.",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Category {
        Name = "Junkfood",
        Image = "Uploads/junkfood.svg",
        Description = "Get your fix of quick, practical, and delicious comfort food. This selection is perfect for when you're on the go or craving a satisfying classic, featuring juicy burgers, crispy french fries, pizza, and other favorites.\n\nThe undeniable appeal often lies in achieving the 'bliss point'—a perfect, crave-worthy combination of salt, sugar, and fat. These foods are deeply tied to positive memories and convenience, offering a delicious and immediate sense of comfort.",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    },
    new Category {
        Name = "Western",
        Image = "Uploads/western.svg",
        Description = "Experience the timeless culinary classics of the West. This category features everything from perfectly cooked, juicy steaks and al dente pastas smothered in rich sauces to fresh, crisp salads and hearty sandwiches.\n\nMany of these dishes are built upon the foundational 'mother sauces' of French cuisine, which provide a rich base for countless variations. The focus remains on high-quality proteins and precise techniques that have defined European and American dining for centuries.",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    }
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
            new MenuCourse { Name = "Tom Yum Thailand", Image = "Uploads/tomyum.svg", Price = 450000, Description = "A classic hot and sour Thai soup filled with fresh seafood, mushrooms, and authentic spices.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Strawberry Float", Image = "Uploads/trawberry.svg", Price = 150000, Description = "A smooth float drink made from creamy vanilla ice cream and real strawberries.", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Chocholate Cookies", Image = "Uploads/cookies.svg", Price = 200000, Description = "Crispy cookies that are rich with premium chocolate chunks.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Green Tea Cheesecake", Image = "Uploads/greentea.svg", Price = 400000, Description = "A smooth cheesecake with a premium matcha flavor on top of a crispy biscuit base.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Soto Banjar Limau Kuit", Image = "Uploads/soto.svg", Price = 150000, Description = "A traditional Banjar soto with shredded chicken, potato cakes (perkedel), and the fresh aroma of kaffir lime.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Italian Spaghetti Bolognese", Image = "Uploads/spaghetti.svg", Price = 450000, Description = "Al dente spaghetti pasta served with a classic bolognese sauce made from meat and tomatoes.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Asian (Lanjutan) ---
            new MenuCourse { Name = "Nasi Goreng Spesial", Image = "Uploads/nasigoreng.png", Price = 55000, Description = "Fried rice with a mix of egg, chicken, shrimp, and vegetables, served with pickles.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Ramen Miso", Image = "Uploads/ramen.png", Price = 75000, Description = "Ramen noodles in a savory miso broth, topped with chicken chashu, a boiled egg, and nori.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Pad Thai", Image = "Uploads/padthai.png", Price = 65000, Description = "Stir-fried Thai rice noodles with a sweet and sour sauce, tofu, shrimp, and a sprinkle of peanuts.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Sate Ayam Madura", Image = "Uploads/sate.png", Price = 45000, Description = "10 skewers of tender chicken satay with a signature Madura peanut sauce, served with rice cakes (lontong).", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Sushi Platter", Image = "Uploads/sushi.jpeg", Price = 120000, Description = "A sushi set containing a salmon roll, california roll, and tuna nigiri.", CategoryId = asianCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Cold Drink (Lanjutan) ---
            new MenuCourse { Name = "Es Kopi Susu Gula Aren", Image = "Uploads/eskopi.svg", Price = 25000, Description = "A blend of espresso, fresh milk, and the natural sweetness of palm sugar (gula aren).", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Jus Mangga Segar", Image = "Uploads/jusmangga.svg", Price = 30000, Description = "Juice from select 'Harum Manis' mangoes with no added sugar.", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Lemon Squash", Image = "Uploads/lemonsquash.svg", Price = 28000, Description = "A refreshing soda drink with real lemon squeeze and a hint of mint leaf.", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Thai Tea Original", Image = "Uploads/thaitea.svg", Price = 22000, Description = "Classic Thai black tea mixed with sweetened condensed milk.", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Es Teh Leci", Image = "Uploads/estehleci.svg", Price = 26000, Description = "Sweet iced tea with added lychee syrup and refreshing lychee fruit.", CategoryId = coldDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Cookies (Lanjutan) ---
            new MenuCourse { Name = "Nastar Premium", Image = "Uploads/nastar.svg", Price = 150000, Description = "Premium cookies filled with homemade pineapple jam, made with a melt-in-your-mouth Wijsman butter dough.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Kaasstengels", Image = "Uploads/kaasstengels.svg", Price = 165000, Description = "Crispy and savory cheese sticks made using authentic Edam cheese.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Putri Salju", Image = "Uploads/putrisalju.svg", Price = 135000, Description = "Crescent-shaped cookies dusted with fine powdered sugar, resembling snow.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Lidah Kucing Rainbow", Image = "Uploads/lidahkucing.svg", Price = 110000, Description = "Thin, crispy 'Cat's Tongue' cookies with an attractive colorful (rainbow) touch.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Oatmeal Raisin Cookies", Image = "Uploads/oatmeal.svg", Price = 125000, Description = "Healthy and delicious cookies made from oatmeal and sweet raisins.", CategoryId = cookiesCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Dessert (Lanjutan) ---
            new MenuCourse { Name = "Lava Cake Cokelat", Image = "Uploads/lavacake.svg", Price = 55000, Description = "A warm chocolate cake with a molten chocolate center, served with a scoop of vanilla ice cream.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Panna Cotta", Image = "Uploads/pannacotta.svg", Price = 45000, Description = "A smooth, creamy Italian pudding served with a fresh strawberry sauce.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Tiramisu", Image = "Uploads/tiramisu.svg", Price = 65000, Description = "A classic Italian coffee dessert with layers of ladyfingers, mascarpone cheese, and a dusting of cocoa powder.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Es Pisang Ijo", Image = "Uploads/pisangijo.svg", Price = 35000, Description = "Banana wrapped in green dough, served with rice flour porridge, syrup, and shaved ice.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Brownies Panggang", Image = "Uploads/brownies.svg", Price = 40000, Description = "A slice of rich, dark chocolate baked brownies with an almond topping.", CategoryId = desertCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Eastern ---
            new MenuCourse { Name = "Kebab Daging Sapi", Image = "Uploads/kebab.svg", Price = 45000, Description = "Spiced grilled beef served in a warm pita bread with fresh vegetables.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Nasi Biryani Ayam", Image = "Uploads/biryani.svg", Price = 70000, Description = "Aromatic basmati rice cooked with rich spices and tender chicken pieces.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Hummus Platter", Image = "Uploads/hummus.svg", Price = 50000, Description = "Smooth chickpea paste served with olive oil and pita bread.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Chicken Shawarma", Image = "Uploads/shawarma.svg", Price = 48000, Description = "A bread wrap filled with sliced grilled chicken, garlic sauce, and pickles.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Falafel", Image = "Uploads/falafel.svg", Price = 40000, Description = "Fried balls made from ground chickpeas, served with tahini sauce.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Umm Ali", Image = "Uploads/ummali.svg", Price = 55000, Description = "A rich, traditional Egyptian bread pudding, served warm with a nut topping.", CategoryId = easternCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // --- Hot Drink ---
            new MenuCourse { Name = "Cappuccino", Image = "Uploads/cappuccino.svg", Price = 35000, Description = "Espresso with steamed milk, topped with a thick layer of milk foam.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Teh Tarik", Image = "Uploads/tehtarik.svg", Price = 20000, Description = "Strong black tea mixed with milk and 'pulled' (tarik) to create a frothy top.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Wedang Jahe", Image = "Uploads/wedangjahe.svg", Price = 18000, Description = "A traditional ginger drink that warms the body, sweetened with palm sugar.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Hot Chocolate", Image = "Uploads/hotchocolate.svg", Price = 32000, Description = "Premium chocolate melted into warm milk, creating a rich and creamy taste.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Matcha Latte", Image = "Uploads/matchalatte.svg", Price = 38000, Description = "High-quality matcha powder whisked with smooth steamed milk.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Kopi Tubruk", Image = "Uploads/kopitubruk.svg", Price = 15000, Description = "A traditional Indonesian black coffee, brewed directly from fine coffee grounds.", CategoryId = hotDrinkCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

            // --- Junkfood ---
            new MenuCourse { Name = "Beef Burger Deluxe", Image = "Uploads/burger.svg", Price = 65000, Description = "A burger with a thick beef patty, cheddar cheese, lettuce, tomato, and a special sauce.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Kentang Goreng", Image = "Uploads/fries.svg", Price = 25000, Description = "Crispy french fries sprinkled with salt, served with ketchup.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Pizza Pepperoni", Image = "Uploads/pizza.svg", Price = 95000, Description = "A thin-crust pizza with tomato sauce, mozzarella cheese, and generous pepperoni.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Ayam Goreng Crispy", Image = "Uploads/friedchicken.svg", Price = 30000, Description = "One piece of fried chicken with a crispy breading and secret spices.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Hotdog", Image = "Uploads/hotdog.svg", Price = 40000, Description = "A soft bun with a grilled beef sausage, mustard, and ketchup.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Onion Rings", Image = "Uploads/onionrings.svg", Price = 32000, Description = "Onion rings fried to a perfect crisp with a special batter.", CategoryId = junkfoodCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // --- Western (Lanjutan) ---
            new MenuCourse { Name = "Sirloin Steak", Image = "Uploads/steak.svg", Price = 185000, Description = "A 200g imported sirloin steak served with mushroom sauce, potatoes, and vegetables.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Chicken Cordon Bleu", Image = "Uploads/cordonbleu.svg", Price = 95000, Description = "A rolled chicken breast filled with smoked beef and melted cheese, fried until golden brown.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Fish and Chips", Image = "Uploads/fishnchips.svg", Price = 75000, Description = "Crispy battered dory fish served with french fries and tartar sauce.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Caesar Salad", Image = "Uploads/caesarsalad.svg", Price = 60000, Description = "A salad with romaine lettuce, croutons, parmesan cheese, and caesar dressing.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new MenuCourse { Name = "Lasagna al Forno", Image = "Uploads/lasagna.svg", Price = 90000, Description = "Layers of pasta with meat sauce, béchamel sauce, and baked mozzarella cheese.", CategoryId = westernCategory.CategoryId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
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
            new Schedule { ScheduleDate = new DateTime(2026, 7, 25), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Schedule { ScheduleDate = new DateTime(2026, 7, 26), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Schedule { ScheduleDate = new DateTime(2026, 7, 27), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Schedule { ScheduleDate = new DateTime(2026, 7, 28), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Schedule { ScheduleDate = new DateTime(2026, 7, 29), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Schedule { ScheduleDate = new DateTime(2026, 7, 30), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }  
        );
        await context.SaveChangesAsync();
      }
      var allSchedules = await context.Schedules.OrderBy(s => s.ScheduleId).ToListAsync();
      var schedule2 = await context.Schedules.OrderBy(s => s.ScheduleId).Skip(1).FirstAsync();

      // === SEED MENU COURSE SCHEDULE ===
      if (!context.MenuCourseSchedules.Any())
      {
        var newEntries = new List<MenuCourseSchedule>();
        foreach (var schedule in allSchedules)
        {
            newEntries.Add(new MenuCourseSchedule
            {
                MenuCourseId = tomyum!.MenuCourseId,
                ScheduleId = schedule.ScheduleId,
                AvailableSlot = 10,
                Status = MSStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        if (allSchedules.Count > 1) 
        {
            newEntries.Add(new MenuCourseSchedule
            {
                MenuCourseId = strawberry.MenuCourseId,
                ScheduleId = allSchedules[1].ScheduleId, 
                AvailableSlot = 8,
                Status = MSStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        
        context.MenuCourseSchedules.AddRange(newEntries);
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
