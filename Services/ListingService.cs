using Microsoft.EntityFrameworkCore;
using webapp_demo.Data;
using webapp_demo.Models;

namespace webapp_demo.Services;

public class ListingService : IListingService
{
    private readonly ApplicationDbContext _db;
    public ListingService(ApplicationDbContext db) => _db = db;

    public async Task<SearchResult> SearchAsync(PropertyFilter f, int page = 1, int pageSize = 10)
    {
        var q = _db.Properties
            .Include(p => p.Images)
            .Include(p => p.PropertyType)
            .Include(p => p.Owner)
            .Where(p => p.Status == PropertyStatus.Approved)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Keyword))
        {
            var kw = f.Keyword.Trim();
            q = q.Where(p => EF.Functions.Like(p.Title, $"%{kw}%")
                || EF.Functions.Like(p.Address, $"%{kw}%")
                || EF.Functions.Like(p.Description, $"%{kw}%"));
        }
        if (!string.IsNullOrWhiteSpace(f.District))
            q = q.Where(p => p.District == f.District);
        if (f.PropertyTypeId.HasValue)
            q = q.Where(p => p.PropertyTypeId == f.PropertyTypeId);
        if (f.MinPrice.HasValue)
            q = q.Where(p => p.Price >= f.MinPrice.Value);
        if (f.MaxPrice.HasValue)
            q = q.Where(p => p.Price <= f.MaxPrice.Value);
        if (f.MinArea.HasValue)
            q = q.Where(p => p.Area >= f.MinArea.Value);
        if (f.MaxArea.HasValue)
            q = q.Where(p => p.Area <= f.MaxArea.Value);
        if (f.Bedrooms.HasValue)
            q = q.Where(p => p.Bedrooms >= f.Bedrooms.Value);
        if (f.IsForRent.HasValue)
            q = q.Where(p => p.IsForRent == f.IsForRent.Value);

        q = f.Sort switch
        {
            "price_asc" => q.OrderBy(p => (double)p.Price),
            "price_desc" => q.OrderByDescending(p => (double)p.Price),
            _ => q.OrderByDescending(p => p.CreatedAt)
        };

        int total = await q.CountAsync();
        page = Math.Max(1, page);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new SearchResult { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<Property?> GetApprovedByIdAsync(int id) =>
        await _db.Properties
            .Include(p => p.Images)
            .Include(p => p.PropertyType)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id && p.Status == PropertyStatus.Approved);

    public async Task<List<Property>> GetFeaturedAsync(int count) =>
        await _db.Properties
            .Where(p => p.Status == PropertyStatus.Approved)
            .Include(p => p.Images)
            .Include(p => p.PropertyType)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task<List<string>> GetDistrictsAsync() =>
        await _db.Properties.Where(p => p.Status == PropertyStatus.Approved)
            .Select(p => p.District).Distinct().OrderBy(d => d).ToListAsync();
}