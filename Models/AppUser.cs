using Microsoft.AspNetCore.Identity;

namespace webapp_demo.Models;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = "";
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
}
