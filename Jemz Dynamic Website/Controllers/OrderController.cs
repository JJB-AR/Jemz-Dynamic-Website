using System.Security.Claims;
using System.Text.Json;
using JemzCoffee.Data;
using JemzCoffee.Models;
using JemzCoffee.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JemzCoffee.Controllers;

[Authorize]
public class OrderController(ApplicationDbContext db) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int menuItemId, string? returnUrl)
    {
        if (!await db.MenuItems.AnyAsync(item => item.Id == menuItemId && item.IsAvailable))
        {
            TempData["Notice"] = "That menu item is no longer available.";
            return RedirectToAction("Index", "Home");
        }

        var cart = ReadCart();
        cart[menuItemId] = Math.Min(50, cart.GetValueOrDefault(menuItemId) + 1);
        SaveCart(cart);
        TempData["Notice"] = "Added to your bag.";
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Cart() => View(await BuildCartAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int menuItemId, int quantity)
    {
        var cart = ReadCart();
        if (quantity <= 0) cart.Remove(menuItemId);
        else if (cart.ContainsKey(menuItemId)) cart[menuItemId] = Math.Clamp(quantity, 1, 50);
        SaveCart(cart);
        await Task.CompletedTask;
        return RedirectToAction(nameof(Cart));
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = await BuildCartAsync();
        if (cart.Items.Count == 0) return RedirectToAction(nameof(Cart));
        return View(new CheckoutViewModel { Cart = cart });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        if (model.FulfillmentType == "Delivery" && string.IsNullOrWhiteSpace(model.DeliveryAddress))
            ModelState.AddModelError(nameof(model.DeliveryAddress), "Enter the address where we should deliver your order.");

        var cart = await BuildCartAsync();
        if (cart.Items.Count == 0)
        {
            TempData["Notice"] = "Your bag is empty. Add a drink before checking out.";
            return RedirectToAction(nameof(Cart));
        }
        if (!ModelState.IsValid)
        {
            model.Cart = cart;
            return View(model);
        }

        var order = new CoffeeOrder
        {
            CustomerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            FulfillmentType = model.FulfillmentType,
            DeliveryAddress = model.FulfillmentType == "Delivery" ? model.DeliveryAddress?.Trim() : null,
            CustomerNote = model.CustomerNote?.Trim(),
            Status = "Received",
            Total = cart.Subtotal,
            Lines = cart.Items.Select(line => new CoffeeOrderLine
            {
                ItemName = line.Item.Name,
                UnitPrice = line.Item.Price,
                Quantity = line.Quantity
            }).ToList()
        };

        db.CoffeeOrders.Add(order);
        await db.SaveChangesAsync();
        HttpContext.Session.Remove("coffee-cart");
        return RedirectToAction(nameof(Confirmation), new { id = order.Id });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await db.CoffeeOrders.AsNoTracking().Include(item => item.Lines)
            .FirstOrDefaultAsync(item => item.Id == id && item.CustomerId == userId);
        return order is null ? NotFound() : View(new OrderReceiptViewModel { Order = order });
    }

    public async Task<IActionResult> History()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var orders = await db.CoffeeOrders.AsNoTracking().Include(item => item.Lines)
            .Where(item => item.CustomerId == userId).OrderByDescending(item => item.CreatedAtUtc).ToListAsync();
        return View(orders);
    }

    private Dictionary<int, int> ReadCart()
    {
        var raw = HttpContext.Session.GetString("coffee-cart");
        if (string.IsNullOrWhiteSpace(raw)) return [];
        try { return JsonSerializer.Deserialize<Dictionary<int, int>>(raw) ?? []; }
        catch (JsonException) { return []; }
    }

    private void SaveCart(Dictionary<int, int> cart) =>
        HttpContext.Session.SetString("coffee-cart", JsonSerializer.Serialize(cart));

    private async Task<CartViewModel> BuildCartAsync()
    {
        var cart = ReadCart();
        if (cart.Count == 0) return new CartViewModel();

        var items = await db.MenuItems.AsNoTracking().Where(item => cart.Keys.Contains(item.Id) && item.IsAvailable)
            .ToListAsync();
        var lines = items.Select(item => new CartLineViewModel
        {
            Item = item,
            Quantity = Math.Clamp(cart.GetValueOrDefault(item.Id), 1, 50)
        }).ToList();
        var availableIds = items.Select(item => item.Id).ToHashSet();
        var staleIds = cart.Keys.Where(id => !availableIds.Contains(id)).ToList();
        if (staleIds.Count > 0)
        {
            foreach (var id in staleIds) cart.Remove(id);
            SaveCart(cart);
        }
        return new CartViewModel { Items = lines };
    }
}
