using System.Text.Json;
using JemzCoffee.Data;
using JemzCoffee.Models;
using JemzCoffee.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JemzCoffee.Controllers;

[Authorize]
public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? q, string? category)
    {
        var query = db.MenuItems.AsNoTracking().Where(item => item.IsAvailable);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(item => item.Name.Contains(q) || item.Description.Contains(q));
        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            query = query.Where(item => item.Category == category);

        var items = await query.OrderBy(item => item.Category).ThenBy(item => item.Name).ToListAsync();
        var categories = await db.MenuItems.AsNoTracking().Where(item => item.IsAvailable)
            .Select(item => item.Category).Distinct().OrderBy(item => item).ToListAsync();
        var shop = await db.ShopProfiles.AsNoTracking().FirstAsync();
        return View(new HomeViewModel
        {
            Shop = shop,
            MenuItems = items,
            Categories = categories,
            Search = q,
            Category = category,
            CartCount = ReadCartCount()
        });
    }

    public async Task<IActionResult> About() => View(await db.ShopProfiles.AsNoTracking().FirstAsync());

    public async Task<IActionResult> Contact() => View(await db.ShopProfiles.AsNoTracking().FirstAsync());

    public IActionResult Error() => View();

    private int ReadCartCount()
    {
        var raw = HttpContext.Session.GetString("coffee-cart");
        if (string.IsNullOrWhiteSpace(raw)) return 0;
        try
        {
            return (JsonSerializer.Deserialize<Dictionary<int, int>>(raw) ?? []).Values.Sum();
        }
        catch (JsonException) { return 0; }
    }
}
