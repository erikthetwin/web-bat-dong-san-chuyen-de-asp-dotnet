using System.ComponentModel.DataAnnotations;

namespace webapp_demo.Models;

public class PropertyImage
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property? Property { get; set; }
    [Required, MaxLength(500)]
    public string ImageUrl { get; set; } = "";
    public bool IsPrimary { get; set; }
}
