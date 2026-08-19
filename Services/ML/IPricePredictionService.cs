using webapp_demo.Models;

namespace webapp_demo.Services.ML;

public interface IPricePredictionService
{
    bool IsModelReady { get; }
    Task TrainIfNeededAsync();
    Task<PredictionResult> PredictAsync(PredictionInput input);
}

public class PredictionInput
{
    public string District { get; set; } = "";
    public string PropertyType { get; set; } = "";
    public float Area { get; set; }
    public float Bedrooms { get; set; }
    public float Bathrooms { get; set; }
    public float Floors { get; set; }
    public float FacadeWidth { get; set; }
    public bool IsForRent { get; set; }
}

public class PredictionResult
{
    public decimal PredictedPrice { get; set; }
    public double R2 { get; set; }
    public double RMSE { get; set; }
    public double MAE { get; set; }
    public string ModelName { get; set; } = "";
    public bool IsReady { get; set; }
}