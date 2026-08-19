using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using webapp_demo.Models;

namespace webapp_demo.Data;

public static class DbSeeder
{
    public static readonly List<(string Name, double Lat, double Lng, decimal PricePerM2)> Districts = new()
    {
        ("Quận 1", 10.776, 106.700, 180000000m),
        ("Quận 2", 10.789, 106.750, 90000000m),
        ("Quận 3", 10.782, 106.680, 160000000m),
        ("Quận 4", 10.762, 106.704, 80000000m),
        ("Quận 5", 10.756, 106.666, 120000000m),
        ("Quận 7", 10.740, 106.708, 95000000m),
        ("Quận 9", 10.838, 106.794, 50000000m),
        ("Bình Thạnh", 10.803, 106.706, 110000000m),
        ("Gò Vấp", 10.836, 106.671, 70000000m),
        ("Thủ Đức", 10.849, 106.754, 65000000m),
        ("Nhà Bè", 10.702, 106.736, 35000000m),
        ("Hóc Môn", 10.889, 106.600, 25000000m)
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        string[] roles = { "Admin", "Seller", "Buyer" };
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        if (await userManager.FindByEmailAsync("admin@demo.com") == null)
        {
            var admin = new AppUser { UserName = "admin@demo.com", Email = "admin@demo.com", EmailConfirmed = true, FullName = "Quản trị viên" };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (!await context.PropertyTypes.AnyAsync())
        {
            string[] names = { "Căn hộ", "Nhà riêng", "Đất", "Nhà mặt tiền", "Mặt bằng kinh doanh", "Biệt thự" };
            foreach (var name in names) context.PropertyTypes.Add(new PropertyType { Name = name });
            await context.SaveChangesAsync();
        }
    }
}
