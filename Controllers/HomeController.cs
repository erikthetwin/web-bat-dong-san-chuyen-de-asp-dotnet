using Microsoft.AspNetCore.Mvc;

namespace webapp_demo.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
