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

    public static async Task SeedListingsAsync(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        if (await context.Properties.AnyAsync()) return;

        var seller = await userManager.FindByEmailAsync("seller@demo.com");
        if (seller == null)
        {
            seller = new AppUser { UserName = "seller@demo.com", Email = "seller@demo.com", EmailConfirmed = true, FullName = "Người bán demo" };
            await userManager.CreateAsync(seller, "Seller@123");
            await userManager.AddToRoleAsync(seller, "Seller");
        }

        var types = await context.PropertyTypes.ToListAsync();
        var rnd = new Random(42);
        string[] streets = { "Nguyễn Văn Linh", "Lê Lợi", "Trần Hưng Đạo", "Hai Bà Trưng", "Võ Văn Ngân", "Nguyễn Thị Định", "Quốc Lộ 50", "Nguyễn Oanh" };
        string[] typeNames = { "Nhà riêng", "Căn hộ", "Đất", "Biệt thự", "Nhà mặt tiền" };

        for (int i = 0; i < 23; i++)
        {
            var d = Districts[rnd.Next(Districts.Count)];
            int nameIdx = rnd.Next(typeNames.Length);
            var t = types.First(x => x.Name == typeNames[nameIdx]);
            decimal area = 40 + rnd.Next(25, 180);
            int bedrooms = rnd.Next(1, 6);
            int bathrooms = Math.Max(1, bedrooms - rnd.Next(0, 2));
            decimal facade = 4 + (decimal)rnd.NextDouble() * 6;
            decimal basePrice = d.PricePerM2 * area;
            decimal typeFactor = t.Name switch
            {
                "Biệt thự" => 1.6m, "Nhà mặt tiền" => 1.8m, "Đất" => 1.1m, "Căn hộ" => 0.9m, _ => 1.0m
            };
            decimal price = basePrice * typeFactor * (1 + 0.03m * bedrooms) + rnd.Next(-20000000, 20000000);
            if (price <= 0) price = 1_000_000m;

            var p = new Property
            {
                Title = $"Bán {(t.Name == "Đất" ? "đất" : t.Name.ToLower())} tại {d.Name}, {streets[rnd.Next(streets.Length)]}",
                Description = $"Bất động sản {t.Name.ToLower()} diện tích {area}m2 tại {d.Name}. Vị trí thuận lợi, hẻm rộng, gần chợ và trường học. Pháp lý rõ ràng, sổ hồng đầy đủ. Liên hệ ngay để xem nhà.",
                Price = price,
                Area = area,
                Bedrooms = bedrooms,
                Bathrooms = bathrooms,
                Floors = 1 + rnd.Next(0, 4),
                FacadeWidth = facade,
                District = d.Name,
                Ward = $"Phường {rnd.Next(1, 20)}",
                Street = streets[rnd.Next(streets.Length)],
                Address = $"{streets[rnd.Next(streets.Length)]}, {d.Name}, TP.HCM",
                Latitude = d.Lat + rnd.NextDouble() * 0.02 - 0.01,
                Longitude = d.Lng + rnd.NextDouble() * 0.02 - 0.01,
                IsForRent = i % 6 == 0,
                ContactPhone = $"09{rnd.Next(10000000, 99999999)}",
                Status = i < 20 ? PropertyStatus.Approved : PropertyStatus.Pending,
                PropertyTypeId = t.Id,
                OwnerId = seller.Id,
                CreatedAt = DateTime.Now.AddDays(-rnd.Next(0, 60))
            };
            context.Properties.Add(p);
            context.PropertyImages.Add(new PropertyImage { Property = p, ImageUrl = $"/uploads/placeholder-{(i % 3) + 1}.svg", IsPrimary = true });
        }
        await context.SaveChangesAsync();
    }
}
