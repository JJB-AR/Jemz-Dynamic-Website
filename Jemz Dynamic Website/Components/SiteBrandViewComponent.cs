using JemzCoffee.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JemzCoffee.Components;

public class SiteBrandViewComponent(ApplicationDbContext db) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var shop = await db.ShopProfiles.AsNoTracking().FirstOrDefaultAsync();
        return View(shop);
    }
}
