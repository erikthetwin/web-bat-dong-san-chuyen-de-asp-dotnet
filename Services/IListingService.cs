using webapp_demo.Models;

namespace webapp_demo.Services;

public interface IListingService
{
    Task<SearchResult> SearchAsync(PropertyFilter filter, int page = 1, int pageSize = 10);
    Task<Property?> GetApprovedByIdAsync(int id);
    Task<List<Property>> GetFeaturedAsync(int count);
    Task<List<string>> GetDistrictsAsync();
}