using JemzCoffee.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JemzCoffee.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<ShopProfile> ShopProfiles => Set<ShopProfile>();
    public DbSet<CoffeeOrder> CoffeeOrders => Set<CoffeeOrder>();
    public DbSet<CoffeeOrderLine> CoffeeOrderLines => Set<CoffeeOrderLine>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<MenuItem>().Property(x => x.Price).HasPrecision(10, 2);
        builder.Entity<CoffeeOrder>().Property(x => x.Total).HasPrecision(10, 2);
        builder.Entity<CoffeeOrderLine>().Property(x => x.UnitPrice).HasPrecision(10, 2);
    }
}
