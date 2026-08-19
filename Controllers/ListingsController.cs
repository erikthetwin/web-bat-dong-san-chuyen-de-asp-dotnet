using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly IContactService _contactService;
    private readonly UserManager<AppUser> _userManager;
    public ListingsController(IListingService listings, ApplicationDbContext db, IContactService contact, UserManager<AppUser> um)
    {
        _listings = listings; _db = db; _contactService = contact; _userManager = um;
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

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(int propertyId, string name, string phone, string? message)
    {
        var prop = await _listings.GetApprovedByIdAsync(propertyId);
        if (prop == null) return NotFound();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
        {
            ModelState.AddModelError("", "Vui lòng nhập họ tên và số điện thoại.");
            return View("Details", prop);
        }
        var userId = _userManager.GetUserId(User);
        await _contactService.CreateAsync(new ContactRequest
        {
            PropertyId = propertyId,
            UserId = userId,
            Name = name.Trim(),
            Phone = phone.Trim(),
            Message = message?.Trim() ?? ""
        });
        TempData["Success"] = "Đã gửi yêu cầu liên hệ. Người bán sẽ liên hệ với bạn.";
        return RedirectToAction("Details", new { id = propertyId });
    }
}