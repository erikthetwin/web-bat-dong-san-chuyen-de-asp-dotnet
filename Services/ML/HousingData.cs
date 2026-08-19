using Microsoft.ML.Data;

namespace webapp_demo.Services.ML;

public class HousingData
{
    [LoadColumn(0)] public string District { get; set; } = "";
    [LoadColumn(1)] public string PropertyType { get; set; } = "";
    [LoadColumn(2)] public float Area { get; set; }
    [LoadColumn(3)] public float Bedrooms { get; set; }
    [LoadColumn(4)] public float Bathrooms { get; set; }
    [LoadColumn(5)] public float Floors { get; set; }
    [LoadColumn(6)] public float FacadeWidth { get; set; }
    [LoadColumn(7)] public float IsForRent { get; set; }
    [LoadColumn(8)] public float Label { get; set; }
}