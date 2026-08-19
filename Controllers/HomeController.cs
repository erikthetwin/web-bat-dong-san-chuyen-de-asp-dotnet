using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapp_demo.Data;
using webapp_demo.Models;

namespace webapp_demo.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var featured = await _db.Properties
            .Where(p => p.Status == PropertyStatus.Approved)
            .Include(p => p.Images)
            .Include(p => p.PropertyType)
            .OrderByDescending(p => p.CreatedAt)
            .Take(6)
            .ToListAsync();
        ViewBag.Types = await _db.PropertyTypes.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();
        ViewBag.Districts = await _db.Properties.Where(p => p.Status == PropertyStatus.Approved)
            .Select(p => p.District).Distinct().OrderBy(d => d).ToListAsync();
        return View(featured);
    }
}
