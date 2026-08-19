using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webapp_demo.Models;

namespace webapp_demo.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(UserManager<AppUser> u, SignInManager<AppUser> s, RoleManager<IdentityRole> r)
    { _userManager = u; _signInManager = s; _roleManager = r; }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var user = new AppUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName,
                PhoneNumber = vm.Phone,
                Address = vm.Address,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                var role = await _roleManager.RoleExistsAsync(vm.Role) ? vm.Role : "Buyer";
                await _userManager.AddToRoleAsync(user, role);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);
        }
        return View(vm);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
        }
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");
        return View(new ProfileViewModel { FullName = user.FullName, Phone = user.PhoneNumber, Address = user.Address });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel vm)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");
        if (ModelState.IsValid)
        {
            user.FullName = vm.FullName;
            user.PhoneNumber = vm.Phone;
            user.Address = vm.Address;
            var upd = await _userManager.UpdateAsync(user);
            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var pw = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);
                if (!pw.Succeeded)
                    foreach (var e in pw.Errors) ModelState.AddModelError("", e.Description);
            }
            if (upd.Succeeded && ModelState.IsValid)
            {
                TempData["Success"] = "Đã cập nhật thông tin.";
                return RedirectToAction("Profile");
            }
            foreach (var e in upd.Errors) ModelState.AddModelError("", e.Description);
        }
        return View(vm);
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}