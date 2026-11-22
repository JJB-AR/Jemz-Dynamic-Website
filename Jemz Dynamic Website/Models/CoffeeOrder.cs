using System.ComponentModel.DataAnnotations;

namespace JemzCoffee.Models;

public class CoffeeOrder
{
    public int Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required, StringLength(12)]
    public string FulfillmentType { get; set; } = "Pickup";

    [StringLength(300)]
    public string? DeliveryAddress { get; set; }

    [StringLength(500)]
    public string? CustomerNote { get; set; }

    [Required, StringLength(24)]
    public string Status { get; set; } = "Received";

    public decimal Total { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<CoffeeOrderLine> Lines { get; set; } = [];
}

public class CoffeeOrderLine
{
    public int Id { get; set; }

    public int CoffeeOrderId { get; set; }

    public CoffeeOrder? CoffeeOrder { get; set; }

    [Required, StringLength(80)]
    public string ItemName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    [Range(1, 50)]
    public int Quantity { get; set; }
}
