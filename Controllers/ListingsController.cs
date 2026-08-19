using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapp_demo.Data;
using webapp_demo.Models;
using webapp_demo.Services;

namespace webapp_demo.Controllers;

public class ListingsController : Controller
{
    private readonly IListingService _listings;
    private readonly ApplicationDbContext _db;
    public ListingsController(IListingService listings, ApplicationDbContext db)
    {
        _listings = listings;
        _db = db;
    }

    public async Task<IActionResult> Index(PropertyFilter filter, int page = 1)
    {
        var result = await _listings.SearchAsync(filter, page);
        ViewBag.Filter = filter;
        ViewBag.Types = await _db.PropertyTypes.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();
        ViewBag.Districts = await _listings.GetDistrictsAsync();
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await _listings.GetApprovedByIdAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }
}