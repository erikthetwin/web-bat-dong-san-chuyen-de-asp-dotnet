using System.ComponentModel.DataAnnotations;

namespace webapp_demo.Models;

public class Property
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public decimal Area { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int Floors { get; set; } = 1;
    public decimal FacadeWidth { get; set; }
    public string District { get; set; } = "";
    public string? Ward { get; set; }
    public string? Street { get; set; }
    public string Address { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsForRent { get; set; }
    public string? ContactPhone { get; set; }
    public PropertyStatus Status { get; set; } = PropertyStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public int PropertyTypeId { get; set; }
    public PropertyType? PropertyType { get; set; }
    public string OwnerId { get; set; } = "";
    public AppUser? Owner { get; set; }
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
}
