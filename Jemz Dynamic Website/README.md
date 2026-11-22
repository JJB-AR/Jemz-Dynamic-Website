# Jemz Coffee

An ASP.NET Core MVC coffee shop ordering site. Customers sign in before browsing, build a bag, then place an order for pickup or delivery. Shop admins manage menu items and shop information, deactivate customer accounts, and view order and sales summaries.

## Run locally

Requirements: .NET 10 SDK and SQL Server LocalDB (included with Visual Studio workloads that install LocalDB).

1. Open a terminal in this folder.
2. Set up the first shop admin credentials with .NET User Secrets:

   ```powershell
   dotnet user-secrets init
   dotnet user-secrets set "AdminSeed:Email" "admin@jemzcoffee.local"
   dotnet user-secrets set "AdminSeed:Password" "Choose-a-strong-password1"
   ```

3. Start the app:

   ```powershell
   dotnet run
   ```

The default connection string uses `(localdb)\MSSQLLocalDB` and creates `JemzCoffeeDb` on first launch. Menu and shop profile starter content are seeded automatically. The first configured admin account is created once; keep its password in User Secrets or an environment variable, not in source control. Customers can create accounts from the sign-in page.

To use another SQL Server instance, override `ConnectionStrings:DefaultConnection` with the normal ASP.NET Core configuration key (for example, the `ConnectionStrings__DefaultConnection` environment variable).

## Main routes

- `/Account/Login` and `/Account/Register`: account entry
- `/Home/Index`: signed-in menu
- `/Order/Cart`, `/Order/Checkout`, `/Order/History`: customer ordering
- `/Admin`: admin statistics and customer account list
- `/Admin/Menu` and `/Admin/Content`: shop content management

The app creates its initial schema with EF Core `EnsureCreated` for a simple local class demo. If you later add schema changes, switch to EF Core migrations before relying on an existing database.
