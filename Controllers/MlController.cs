using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapp_demo.Data;
using webapp_demo.Services.ML;

namespace webapp_demo.Controllers;

public class MlController : Controller
{
    private readonly IPricePredictionService _ml;
    private readonly ApplicationDbContext _db;
    public MlController(IPricePredictionService ml, ApplicationDbContext db) { _ml = ml; _db = db; }

    [HttpGet]
    public async Task<IActionResult> Predict()
    {
        ViewBag.Districts = DbSeeder.Districts.Select(d => d.Name).ToList();
        ViewBag.Types = (await _db.PropertyTypes.Where(t => t.IsActive).ToListAsync()).Select(t => t.Name).ToList();
        ViewBag.Ready = _ml.IsModelReady;
        return View(new PredictionInput { District = "Quận 1", PropertyType = "Căn hộ", Area = 70, Bedrooms = 2, Bathrooms = 2, Floors = 3, FacadeWidth = 5 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Predict(PredictionInput input)
    {
        ViewBag.Districts = DbSeeder.Districts.Select(d => d.Name).ToList();
        ViewBag.Types = (await _db.PropertyTypes.Where(t => t.IsActive).ToListAsync()).Select(t => t.Name).ToList();
        ViewBag.Ready = _ml.IsModelReady;
        if (!_ml.IsModelReady)
        {
            ViewBag.Error = "Mô hình chưa sẵn sàng.";
            return View(input);
        }
        var result = await _ml.PredictAsync(input);
        ViewData["Result"] = result;
        return View(input);
    }
}