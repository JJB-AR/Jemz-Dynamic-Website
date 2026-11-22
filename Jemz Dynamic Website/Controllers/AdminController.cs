using JemzCoffee.Data;
using JemzCoffee.Models;
using JemzCoffee.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JemzCoffee.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var allUsers = (await users.GetUsersInRoleAsync("Customer")).OrderByDescending(user => user.JoinedAtUtc).ToList();
        var model = new AdminDashboardViewModel
        {
            ActiveUsers = allUsers.Count(user => user.IsActive),
            InactiveUsers = allUsers.Count(user => !user.IsActive),
            OrdersToday = await db.CoffeeOrders.CountAsync(order => order.CreatedAtUtc >= today),
            SalesToday = await db.CoffeeOrders.Where(order => order.CreatedAtUtc >= today)
                .Select(order => (decimal?)order.Total).SumAsync() ?? 0m,
            Users = allUsers.Select(user => new AdminUserRow
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                JoinedAtUtc = user.JoinedAtUtc
            }).ToList(),
            RecentOrders = await db.CoffeeOrders.AsNoTracking().Include(order => order.Lines)
                .OrderByDescending(order => order.CreatedAtUtc).Take(6).ToListAsync()
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        if (user.Id == users.GetUserId(User))
        {
            TempData["Notice"] = "You cannot deactivate your own account.";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = !user.IsActive;
        var result = await users.UpdateAsync(user);
        if (result.Succeeded && !user.IsActive)
            await users.UpdateSecurityStampAsync(user);
        TempData["Notice"] = result.Succeeded
            ? user.IsActive ? "Account activated." : "Account deactivated."
            : string.Join(" ", result.Errors.Select(error => error.Description));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Menu() => View(await db.MenuItems.AsNoTracking().OrderBy(item => item.Category)
        .ThenBy(item => item.Name).ToListAsync());

    public IActionResult NewMenuItem() => View("EditMenuItem", new MenuItemInput());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NewMenuItem(MenuItemInput model)
    {
        if (!ModelState.IsValid) return View("EditMenuItem", model);
        db.MenuItems.Add(new MenuItem
        {
            Name = model.Name.Trim(), Description = model.Description.Trim(), Category = model.Category.Trim(),
            Icon = model.Icon.Trim(), Price = model.Price
        });
        await db.SaveChangesAsync();
        TempData["Notice"] = "Menu item added.";
        return RedirectToAction(nameof(Menu));
    }

    public async Task<IActionResult> EditMenuItem(int id)
    {
        var item = await db.MenuItems.FindAsync(id);
        if (item is null) return NotFound();
        return View(new MenuItemInput
        {
            Id = item.Id, Name = item.Name, Description = item.Description,
            Category = item.Category, Icon = item.Icon, Price = item.Price
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMenuItem(MenuItemInput model)
    {
        if (!ModelState.IsValid) return View(model);
        var item = await db.MenuItems.FindAsync(model.Id);
        if (item is null) return NotFound();
        item.Name = model.Name.Trim();
        item.Description = model.Description.Trim();
        item.Category = model.Category.Trim();
        item.Icon = model.Icon.Trim();
        item.Price = model.Price;
        await db.SaveChangesAsync();
        TempData["Notice"] = "Menu item saved.";
        return RedirectToAction(nameof(Menu));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleMenuItem(int id)
    {
        var item = await db.MenuItems.FindAsync(id);
        if (item is null) return NotFound();
        item.IsAvailable = !item.IsAvailable;
        await db.SaveChangesAsync();
        TempData["Notice"] = item.IsAvailable ? "Menu item is available." : "Menu item hidden from the menu.";
        return RedirectToAction(nameof(Menu));
    }

    public async Task<IActionResult> Content()
    {
        var shop = await db.ShopProfiles.FirstAsync();
        return View(ToInput(shop));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Content(ShopProfileInput model)
    {
        if (!ModelState.IsValid) return View(model);
        var shop = await db.ShopProfiles.FirstAsync();
        shop.BrandName = model.BrandName.Trim();
        shop.Tagline = model.Tagline.Trim();
        shop.About = model.About.Trim();
        shop.Owners = model.Owners.Trim();
        shop.ContactEmail = model.ContactEmail.Trim();
        shop.ContactPhone = model.ContactPhone.Trim();
        shop.Address = model.Address.Trim();
        shop.OpeningHours = model.OpeningHours.Trim();
        await db.SaveChangesAsync();
        TempData["Notice"] = "Shop information saved.";
        return RedirectToAction(nameof(Content));
    }

    private static ShopProfileInput ToInput(ShopProfile shop) => new()
    {
        BrandName = shop.BrandName,
        Tagline = shop.Tagline,
        About = shop.About,
        Owners = shop.Owners,
        ContactEmail = shop.ContactEmail,
        ContactPhone = shop.ContactPhone,
        Address = shop.Address,
        OpeningHours = shop.OpeningHours
    };
}
