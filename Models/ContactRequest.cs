using System.ComponentModel.DataAnnotations;

namespace webapp_demo.Models;

public class ContactRequest
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property? Property { get; set; }
    public string? UserId { get; set; }
    public AppUser? User { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";
    [Required, MaxLength(20)]
    public string Phone { get; set; } = "";
    [MaxLength(1000)]
    public string Message { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
