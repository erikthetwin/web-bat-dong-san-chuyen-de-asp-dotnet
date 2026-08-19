using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webapp_demo.Models;
using webapp_demo.Services;

namespace webapp_demo.Controllers;

[Authorize(Roles = "Buyer,Seller")]
public class FavoritesController : Controller
{
    private readonly IFavoriteService _favorites;
    private readonly UserManager<AppUser> _userManager;
    public FavoritesController(IFavoriteService f, UserManager<AppUser> u) { _favorites = f; _userManager = u; }

    public async Task<IActionResult> Index()
    {
        var items = await _favorites.GetSavedAsync(_userManager.GetUserId(User)!);
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int propertyId)
    {
        var saved = await _favorites.ToggleAsync(_userManager.GetUserId(User)!, propertyId);
        TempData["Success"] = saved ? "Đã lưu tin." : "Đã bỏ lưu tin.";
        var referer = Request.Headers["Referer"].ToString();
        return Redirect(string.IsNullOrEmpty(referer) ? "/" : referer);
    }
}