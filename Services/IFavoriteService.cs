using webapp_demo.Models;

namespace webapp_demo.Services;

public interface IFavoriteService
{
    Task<bool> ToggleAsync(string userId, int propertyId);
    Task<bool> IsSavedAsync(string userId, int propertyId);
    Task<List<Property>> GetSavedAsync(string userId);
}