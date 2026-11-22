using JemzCoffee.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JemzCoffee.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (!await db.ShopProfiles.AnyAsync())
        {
            db.ShopProfiles.Add(new ShopProfile
            {
                BrandName = "Jemz Coffee",
                Tagline = "Good coffee. Good people. Your way.",
                About = "Jemz is your neighborhood coffee stop for carefully made drinks, a friendly hello and a slower moment in a busy day. Order ahead for a quick pickup or have your favorites brought to your door.",
                Owners = "Jemz Coffee Team",
                ContactEmail = "hello@jemzcoffee.example",
                ContactPhone = "+63 917 000 0000",
                Address = "Quezon City, Metro Manila",
                OpeningHours = "Daily, 7:00 AM–8:00 PM"
            });
        }

        if (!await db.MenuItems.AnyAsync())
        {
            db.MenuItems.AddRange(
                new MenuItem { Name = "Brown Sugar Latte", Description = "Double espresso, silky milk and house brown sugar syrup.", Category = "Coffee", Icon = "🥛", Price = 165 },
                new MenuItem { Name = "Cloud Cappuccino", Description = "A rich espresso tucked under a soft cloud of milk foam.", Category = "Coffee", Icon = "☁️", Price = 150 },
                new MenuItem { Name = "Honey Oat Cold Brew", Description = "Slow-steeped coffee, oat milk and a touch of local honey.", Category = "Cold brew", Icon = "🧊", Price = 175 },
                new MenuItem { Name = "Iced Spanish Latte", Description = "Bold espresso with sweetened milk, served over ice.", Category = "Cold brew", Icon = "🧋", Price = 170 },
                new MenuItem { Name = "Strawberry Matcha", Description = "Ceremonial matcha layered with strawberry and fresh milk.", Category = "Tea & matcha", Icon = "🍵", Price = 185 },
                new MenuItem { Name = "Almond Croissant", Description = "Flaky, buttery pastry filled with almond cream.", Category = "Bakery", Icon = "🥐", Price = 125 }
            );
        }

        await db.SaveChangesAsync();

        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Customer", "Admin" })
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new IdentityRole(role));
            }
        }

        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            var users = services.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await users.FindByEmailAsync(email);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = "Shop administrator",
                    IsActive = true
                };
                var result = await users.CreateAsync(admin, password);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException("The configured admin account could not be created: " +
                        string.Join("; ", result.Errors.Select(error => error.Description)));
                }
            }

            if (!await users.IsInRoleAsync(admin, "Admin"))
            {
                await users.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
