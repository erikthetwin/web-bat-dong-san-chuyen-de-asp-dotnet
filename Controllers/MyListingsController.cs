using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapp_demo.Data;
using webapp_demo.Models;

namespace webapp_demo.Controllers;

[Authorize(Roles = "Seller")]
public class MyListingsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _env;
    public MyListingsController(ApplicationDbContext db, UserManager<AppUser> u, IWebHostEnvironment env)
    { _db = db; _userManager = u; _env = env; }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var items = await _db.Properties
            .Include(p => p.Images)
            .Include(p => p.PropertyType)
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Types = await _db.PropertyTypes.Where(t => t.IsActive).ToListAsync();
        ViewBag.Districts = DbSeeder.Districts.Select(d => d.Name).ToList();
        return View(new PropertyFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var userId = _userManager.GetUserId(User)!;
            var user = await _userManager.GetUserAsync(User);
            var prop = new Property
            {
                Title = vm.Title, Description = vm.Description, Price = vm.Price, Area = vm.Area,
                Bedrooms = vm.Bedrooms, Bathrooms = vm.Bathrooms, Floors = vm.Floors, FacadeWidth = vm.FacadeWidth,
                District = vm.District, Ward = vm.Ward, Street = vm.Street, Address = vm.Address,
                Latitude = vm.Latitude, Longitude = vm.Longitude, IsForRent = vm.IsForRent,
                ContactPhone = string.IsNullOrWhiteSpace(vm.ContactPhone) ? user?.PhoneNumber : vm.ContactPhone,
                PropertyTypeId = vm.PropertyTypeId, OwnerId = userId,
                Status = PropertyStatus.Pending
            };
            _db.Properties.Add(prop);
            await _db.SaveChangesAsync();

            bool first = true;
            foreach (var f in vm.Images ?? new List<IFormFile>())
            {
                if (f.Length == 0 || f.Length > 5 * 1024 * 1024) continue;
                var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp") continue;
                var filename = Guid.NewGuid().ToString("N") + ext;
                var path = Path.Combine(_env.WebRootPath, "uploads", filename);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await using var stream = System.IO.File.Create(path);
                await f.CopyToAsync(stream);
                _db.PropertyImages.Add(new PropertyImage { PropertyId = prop.Id, ImageUrl = "/uploads/" + filename, IsPrimary = first });
                first = false;
            }
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã đăng tin. Tin đang chờ quản trị viên duyệt.";
            return RedirectToAction("Index");
        }
        ViewBag.Types = await _db.PropertyTypes.Where(t => t.IsActive).ToListAsync();
        ViewBag.Districts = DbSeeder.Districts.Select(d => d.Name).ToList();
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var p = await _db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId);
        if (p == null) return NotFound();
        var vm = new PropertyFormViewModel
        {
            Id = p.Id, Title = p.Title, Description = p.Description, Price = p.Price, Area = p.Area,
            Bedrooms = p.Bedrooms, Bathrooms = p.Bathrooms, Floors = p.Floors, FacadeWidth = p.FacadeWidth,
            District = p.District, Ward = p.Ward, Street = p.Street, Address = p.Address,
            Latitude = p.Latitude, Longitude = p.Longitude, IsForRent = p.IsForRent,
            ContactPhone = p.ContactPhone, PropertyTypeId = p.PropertyTypeId
        };
        ViewBag.Types = await _db.PropertyTypes.Where(t => t.IsActive).ToListAsync();
        ViewBag.Districts = DbSeeder.Districts.Select(d => d.Name).ToList();
        ViewBag.ExistingImages = p.Images;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PropertyFormViewModel vm)
    {
        var userId = _userManager.GetUserId(User)!;
        var p = await _db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == vm.Id && x.OwnerId == userId);
        if (p == null) return NotFound();
        if (ModelState.IsValid)
        {
            p.Title = vm.Title; p.Description = vm.Description; p.Price = vm.Price; p.Area = vm.Area;
            p.Bedrooms = vm.Bedrooms; p.Bathrooms = vm.Bathrooms; p.Floors = vm.Floors; p.FacadeWidth = vm.FacadeWidth;
            p.District = vm.District; p.Ward = vm.Ward; p.Street = vm.Street; p.Address = vm.Address;
            p.Latitude = vm.Latitude; p.Longitude = vm.Longitude; p.IsForRent = vm.IsForRent;
            p.ContactPhone = vm.ContactPhone; p.PropertyTypeId = vm.PropertyTypeId;
            p.Status = PropertyStatus.Pending;
            p.UpdatedAt = DateTime.Now;

            bool first = !p.Images.Any();
            foreach (var f in vm.Images ?? new List<IFormFile>())
            {
                if (f.Length == 0 || f.Length > 5 * 1024 * 1024) continue;
                var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp") continue;
                var filename = Guid.NewGuid().ToString("N") + ext;
                var path = Path.Combine(_env.WebRootPath, "uploads", filename);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await using var stream = System.IO.File.Create(path);
                await f.CopyToAsync(stream);
                _db.PropertyImages.Add(new PropertyImage { PropertyId = p.Id, ImageUrl = "/uploads/" + filename, IsPrimary = first });
                first = false;
            }
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật tin. Tin cần được duyệt lại.";
            return RedirectToAction("Index");
        }
        ViewBag.Types = await _db.PropertyTypes.Where(t => t.IsActive).ToListAsync();
        ViewBag.Districts = DbSeeder.Districts.Select(d => d.Name).ToList();
        ViewBag.ExistingImages = p.Images;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var p = await _db.Properties.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId);
        if (p != null)
        {
            _db.Properties.Remove(p);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int imageId, int propertyId)
    {
        var userId = _userManager.GetUserId(User)!;
        var img = await _db.PropertyImages
            .Include(i => i.Property)
            .FirstOrDefaultAsync(i => i.Id == imageId && i.Property!.OwnerId == userId);
        if (img != null)
        {
            _db.PropertyImages.Remove(img);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Edit", new { id = propertyId });
    }
}