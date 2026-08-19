using Microsoft.EntityFrameworkCore;
using webapp_demo.Data;
using webapp_demo.Models;

namespace webapp_demo.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _db;
    public AdminService(ApplicationDbContext db) => _db = db;

    public async Task<AdminStats> GetStatsAsync()
    {
        var all = _db.Properties;
        var stats = new AdminStats
        {
            TotalListings = await all.CountAsync(),
            Pending = await all.CountAsync(p => p.Status == PropertyStatus.Pending),
            Approved = await all.CountAsync(p => p.Status == PropertyStatus.Approved),
            Rejected = await all.CountAsync(p => p.Status == PropertyStatus.Rejected),
            Banned = await all.CountAsync(p => p.Status == PropertyStatus.Banned),
            Sold = await all.CountAsync(p => p.Status == PropertyStatus.Sold),
            TotalUsers = await _db.Users.CountAsync(),
            TotalContacts = await _db.ContactRequests.CountAsync(),
            ByType = (await _db.Properties.GroupBy(p => p.PropertyType!.Name)
                .Select(g => new { Name = g.Key, C = g.Count() })
                .OrderByDescending(x => x.C)
                .ToListAsync())
                .Select(x => (x.Name, x.C))
                .ToList(),
            ByDistrict = (await _db.Properties.GroupBy(p => p.District)
                .Select(g => new { Name = g.Key, C = g.Count() })
                .OrderByDescending(x => x.C)
                .ToListAsync())
                .Select(x => (x.Name, x.C))
                .ToList()
        };
        return stats;
    }
}