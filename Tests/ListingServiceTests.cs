using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using webapp_demo.Data;
using webapp_demo.Models;
using webapp_demo.Services;
using Xunit;

namespace webapp_demo.Tests;

public class ListingServiceTests
{
    private static async Task<(ListingService service, SqliteConnection conn)> CreateServiceAsync()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(conn).Options;
        var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var owner = new AppUser { Id = Guid.NewGuid().ToString(), UserName = "o@x.com", Email = "o@x.com", FullName = "O" };
        var type1 = new PropertyType { Name = "Căn hộ" };
        var type2 = new PropertyType { Name = "Đất" };
        db.Users.Add(owner);
        db.PropertyTypes.AddRange(type1, type2);
        db.Properties.AddRange(
            new Property { Title = "Căn hộ Quận 1 đẹp", Description = "gần chợ", Price = 3_000_000_000m, Area = 80m, Bedrooms = 2, District = "Quận 1", Status = PropertyStatus.Approved, OwnerId = owner.Id, PropertyType = type1 },
            new Property { Title = "Đất Gò Vấp rộng", Description = "mặt tiền", Price = 4_500_000_000m, Area = 120m, Bedrooms = 0, District = "Gò Vấp", Status = PropertyStatus.Approved, OwnerId = owner.Id, PropertyType = type2 },
            new Property { Title = "Nhà Bình Thạnh", Description = "cũ", Price = 1_000_000_000m, Area = 50m, Bedrooms = 1, District = "Bình Thạnh", Status = PropertyStatus.Pending, OwnerId = owner.Id, PropertyType = type1 }
        );
        await db.SaveChangesAsync();
        return (new ListingService(db), conn);
    }

    [Fact]
    public async Task Search_ReturnsOnlyApproved()
    {
        var (service, _) = await CreateServiceAsync();
        var result = await service.SearchAsync(new PropertyFilter());
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, p => Assert.Equal(PropertyStatus.Approved, p.Status));
    }

    [Fact]
    public async Task Search_KeywordMatches_Title_Address_Description()
    {
        var (service, _) = await CreateServiceAsync();
        var result = await service.SearchAsync(new PropertyFilter { Keyword = "gần chợ" });
        Assert.Single(result.Items);
        Assert.Equal("Căn hộ Quận 1 đẹp", result.Items[0].Title);
    }

    [Fact]
    public async Task Search_Filters_By_District_Price_Area()
    {
        var (service, _) = await CreateServiceAsync();
        var result = await service.SearchAsync(new PropertyFilter
        {
            District = "Quận 1",
            MinPrice = 2_000_000_000m,
            MaxPrice = 4_000_000_000m,
            MinArea = 60m,
            MaxArea = 100m
        });
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task Search_Pages_Results()
    {
        var (service, _) = await CreateServiceAsync();
        var result = await service.SearchAsync(new PropertyFilter(), page: 1, pageSize: 1);
        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task Search_Sorts_By_Price_Asc()
    {
        var (service, _) = await CreateServiceAsync();
        var result = await service.SearchAsync(new PropertyFilter { Sort = "price_asc" });
        Assert.Equal(3_000_000_000m, result.Items[0].Price);
    }
}