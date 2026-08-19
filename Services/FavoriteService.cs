using Microsoft.EntityFrameworkCore;
using webapp_demo.Data;
using webapp_demo.Models;

namespace webapp_demo.Services;

public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _db;
    public FavoriteService(ApplicationDbContext db) => _db = db;

    public async Task<bool> ToggleAsync(string userId, int propertyId)
    {
        var existing = await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId);
        if (existing != null)
        {
            _db.Favorites.Remove(existing);
            await _db.SaveChangesAsync();
            return false;
        }
        _db.Favorites.Add(new Favorite { UserId = userId, PropertyId = propertyId });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsSavedAsync(string userId, int propertyId) =>
        await _db.Favorites.AnyAsync(f => f.UserId == userId && f.PropertyId == propertyId);

    public async Task<List<Property>> GetSavedAsync(string userId) =>
        await _db.Favorites
            .Include(f => f.Property).ThenInclude(p => p!.Images)
            .Include(f => f.Property).ThenInclude(p => p!.PropertyType)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.SavedAt)
            .Select(f => f.Property!)
            .ToListAsync();
}