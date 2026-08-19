using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapp_demo.Data;
using webapp_demo.Models;
using webapp_demo.Services;

namespace webapp_demo.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _admin;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public AdminController(IAdminService admin, ApplicationDbContext db, UserManager<AppUser> u, RoleManager<IdentityRole> r)
    { _admin = admin; _db = db; _userManager = u; _roleManager = r; }

    public async Task<IActionResult> Index()
    {
        var stats = await _admin.GetStatsAsync();
        return View(stats);
    }

    public async Task<IActionResult> Moderation()
    {
        var items = await _db.Properties
            .Include(p => p.Images)
            .Include(p => p.PropertyType)
            .Include(p => p.Owner)
            .Where(p => p.Status != PropertyStatus.Approved && p.Status != PropertyStatus.Sold)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var p = await _db.Properties.FindAsync(id);
        if (p != null) { p.Status = PropertyStatus.Approved; p.UpdatedAt = DateTime.Now; await _db.SaveChangesAsync(); }
        return RedirectToAction("Moderation");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var p = await _db.Properties.FindAsync(id);
        if (p != null) { p.Status = PropertyStatus.Rejected; p.UpdatedAt = DateTime.Now; await _db.SaveChangesAsync(); }
        return RedirectToAction("Moderation");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ban(int id)
    {
        var p = await _db.Properties.FindAsync(id);
        if (p != null) { p.Status = PropertyStatus.Banned; p.UpdatedAt = DateTime.Now; await _db.SaveChangesAsync(); }
        return RedirectToAction("Moderation");
    }

    public async Task<IActionResult> Users()
    {
        var users = new List<(AppUser User, List<string> Roles, bool Locked)>();
        foreach (var u in await _db.Users.OrderBy(x => x.Email).ToListAsync())
        {
            users.Add((u, (await _userManager.GetRolesAsync(u)).ToList(), await _userManager.IsLockedOutAsync(u)));
        }
        ViewBag.Roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockUser(string id)
    {
        var u = await _userManager.FindByIdAsync(id);
        if (u != null && !await _userManager.IsInRoleAsync(u, "Admin"))
        {
            await _userManager.SetLockoutEnabledAsync(u, true);
            await _userManager.SetLockoutEndDateAsync(u, DateTimeOffset.MaxValue);
        }
        return RedirectToAction("Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnblockUser(string id)
    {
        var u = await _userManager.FindByIdAsync(id);
        if (u != null)
            await _userManager.SetLockoutEndDateAsync(u, null);
        return RedirectToAction("Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string id, string role)
    {
        var u = await _userManager.FindByIdAsync(id);
        if (u != null && !await _userManager.IsInRoleAsync(u, "Admin") && await _roleManager.RoleExistsAsync(role))
        {
            var current = await _userManager.GetRolesAsync(u);
            await _userManager.RemoveFromRolesAsync(u, current);
            await _userManager.AddToRoleAsync(u, role);
        }
        return RedirectToAction("Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var u = await _userManager.FindByIdAsync(id);
        if (u != null && !await _userManager.IsInRoleAsync(u, "Admin"))
            await _userManager.DeleteAsync(u);
        return RedirectToAction("Users");
    }
}