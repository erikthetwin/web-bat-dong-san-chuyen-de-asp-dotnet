namespace webapp_demo.Models;

public class Favorite
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public AppUser? User { get; set; }
    public int PropertyId { get; set; }
    public Property? Property { get; set; }
    public DateTime SavedAt { get; set; } = DateTime.Now;
}
