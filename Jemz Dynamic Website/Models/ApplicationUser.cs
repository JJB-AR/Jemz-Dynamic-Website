using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace JemzCoffee.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}
