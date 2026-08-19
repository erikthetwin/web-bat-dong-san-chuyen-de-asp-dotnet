namespace webapp_demo.Services;

public interface IAdminService
{
    Task<AdminStats> GetStatsAsync();
}

public class AdminStats
{
    public int TotalListings { get; set; }
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Banned { get; set; }
    public int Sold { get; set; }
    public int TotalUsers { get; set; }
    public int TotalContacts { get; set; }
    public List<(string Type, int Count)> ByType { get; set; } = new();
    public List<(string District, int Count)> ByDistrict { get; set; } = new();
}