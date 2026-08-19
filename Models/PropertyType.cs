using System.ComponentModel.DataAnnotations;

namespace webapp_demo.Models;

public class PropertyType
{
    public int Id { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
