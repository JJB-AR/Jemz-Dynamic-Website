using System.ComponentModel.DataAnnotations;

namespace JemzCoffee.Models;

public class ShopProfile
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string BrandName { get; set; } = "Jemz Coffee";

    [Required, StringLength(160)]
    public string Tagline { get; set; } = "A little joy, one cup at a time.";

    [Required, StringLength(1200)]
    public string About { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Owners { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string ContactEmail { get; set; } = string.Empty;

    [Required, Phone, StringLength(40)]
    public string ContactPhone { get; set; } = string.Empty;

    [Required, StringLength(180)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string OpeningHours { get; set; } = string.Empty;
}
