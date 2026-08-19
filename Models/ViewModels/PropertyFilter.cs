namespace webapp_demo.Models;

public class PropertyFilter
{
    public string? Keyword { get; set; }
    public string? District { get; set; }
    public int? PropertyTypeId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }
    public int? Bedrooms { get; set; }
    public bool? IsForRent { get; set; }
    public string Sort { get; set; } = "newest"; // newest | price_asc | price_desc
}