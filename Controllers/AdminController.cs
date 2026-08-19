using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapp_demo.Services;

namespace webapp_demo.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _admin;
    public AdminController(IAdminService admin) => _admin = admin;

    public async Task<IActionResult> Index()
    {
        var stats = await _admin.GetStatsAsync();
        return View(stats);
    }
}