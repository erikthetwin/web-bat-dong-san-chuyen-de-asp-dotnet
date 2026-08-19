using Microsoft.AspNetCore.Authorization;
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
    public AdminController(IAdminService admin, ApplicationDbContext db) { _admin = admin; _db = db; }

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
}