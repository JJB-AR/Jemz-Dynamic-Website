using System.ComponentModel.DataAnnotations;
using JemzCoffee.Models;

namespace JemzCoffee.ViewModels;

public class HomeViewModel
{
    public ShopProfile Shop { get; set; } = new();
    public IReadOnlyList<MenuItem> MenuItems { get; set; } = [];
    public IReadOnlyList<string> Categories { get; set; } = [];
    public string? Search { get; set; }
    public string? Category { get; set; }
    public int CartCount { get; set; }
}

public class CartLineViewModel
{
    public MenuItem Item { get; set; } = new();
    public int Quantity { get; set; }
    public decimal LineTotal => Item.Price * Quantity;
}

public class CartViewModel
{
    public IReadOnlyList<CartLineViewModel> Items { get; set; } = [];
    public decimal Subtotal => Items.Sum(line => line.LineTotal);
}

public class CheckoutViewModel
{
    [Required]
    [RegularExpression("^(Pickup|Delivery)$", ErrorMessage = "Choose pickup or delivery.")]
    [Display(Name = "How would you like your order?")]
    public string FulfillmentType { get; set; } = "Pickup";

    [StringLength(300), Display(Name = "Delivery address")]
    public string? DeliveryAddress { get; set; }

    [StringLength(500), Display(Name = "Order note")]
    public string? CustomerNote { get; set; }

    public CartViewModel Cart { get; set; } = new();
}

public class ShopProfileInput
{
    [Required, StringLength(80), Display(Name = "Shop name")]
    public string BrandName { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Tagline { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string About { get; set; } = string.Empty;

    [Required, StringLength(120), Display(Name = "Owners")]
    public string Owners { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160), Display(Name = "Contact email")]
    public string ContactEmail { get; set; } = string.Empty;

    [Required, Phone, StringLength(40), Display(Name = "Contact phone")]
    public string ContactPhone { get; set; } = string.Empty;

    [Required, StringLength(180)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(120), Display(Name = "Opening hours")]
    public string OpeningHours { get; set; } = string.Empty;
}

public class MenuItemInput
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(280)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Category { get; set; } = "Coffee";

    [Required, StringLength(8), Display(Name = "Card emoji")]
    public string Icon { get; set; } = "☕";

    [Range(0.01, 9999), DataType(DataType.Currency)]
    public decimal Price { get; set; }
}

public class AdminDashboardViewModel
{
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int OrdersToday { get; set; }
    public decimal SalesToday { get; set; }
    public IReadOnlyList<AdminUserRow> Users { get; set; } = [];
    public IReadOnlyList<CoffeeOrder> RecentOrders { get; set; } = [];
}

public class AdminUserRow
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime JoinedAtUtc { get; set; }
}

public class OrderReceiptViewModel
{
    public CoffeeOrder Order { get; set; } = new();
}
