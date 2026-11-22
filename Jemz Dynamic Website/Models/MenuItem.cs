using System.ComponentModel.DataAnnotations;

namespace JemzCoffee.Models;

public class MenuItem
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(280)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Category { get; set; } = "Coffee";

    [Required, StringLength(8)]
    public string Icon { get; set; } = "☕";

    [Range(0.01, 9999)]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;
}
