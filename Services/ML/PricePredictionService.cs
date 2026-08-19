using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;

namespace webapp_demo.Services.ML;

public class PricePredictionService : IPricePredictionService
{
    private readonly string _modelPath;
    private MLContext? _ml;
    private ITransformer? _model;
    private double _r2, _rmse, _mae;
    public bool IsModelReady => _model != null;

    public PricePredictionService(IWebHostEnvironment env)
    {
        _modelPath = Path.Combine(env.ContentRootPath, "ML", "model.zip");
    }

    public Task TrainIfNeededAsync()
    {
        if (File.Exists(_modelPath)) return LoadAsync();
        return Task.Run(Train);
    }

    private void Train()
    {
        _ml = new MLContext(seed: 1);
        var data = _ml.Data.LoadFromEnumerable(DatasetGenerator.Generate(1200));
        var split = _ml.Data.TrainTestSplit(data, testFraction: 0.2);

        var pipeline = _ml.Transforms.Categorical.OneHotEncoding("District")
            .Append(_ml.Transforms.Categorical.OneHotEncoding("PropertyType"))
            .Append(_ml.Transforms.Concatenate("Features",
                "District", "PropertyType", "Area", "Bedrooms", "Bathrooms", "Floors", "FacadeWidth", "IsForRent"))
            .Append(_ml.Transforms.CopyColumns("Label", "Label"))
            .Append(_ml.Regression.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features", numberOfLeaves: 20, numberOfTrees: 100, minimumExampleCountPerLeaf: 10));

        var model = pipeline.Fit(split.TrainSet);
        var metrics = _ml.Regression.Evaluate(model.Transform(split.TestSet), labelColumnName: "Label");
        _r2 = metrics.RSquared;
        _rmse = metrics.RootMeanSquaredError;
        _mae = metrics.MeanAbsoluteError;

        Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);
        _ml.Model.Save(model, split.TrainSet.Schema, _modelPath);
        _model = model;
    }

    private Task LoadAsync()
    {
        _ml = new MLContext(seed: 1);
        _model = _ml.Model.Load(_modelPath, out _);
        return Task.CompletedTask;
    }

    public Task<PredictionResult> PredictAsync(PredictionInput input)
    {
        if (_ml == null || _model == null)
            return Task.FromResult(new PredictionResult { IsReady = false });
        var predictionEngine = _ml.Model.CreatePredictionEngine<HousingData, HousingPrediction>(_model);
        var sample = new HousingData
        {
            District = input.District,
            PropertyType = input.PropertyType,
            Area = input.Area,
            Bedrooms = input.Bedrooms,
            Bathrooms = input.Bathrooms,
            Floors = input.Floors,
            FacadeWidth = input.FacadeWidth,
            IsForRent = input.IsForRent ? 1 : 0,
            Label = 0
        };
        var pred = predictionEngine.Predict(sample);
        return Task.FromResult(new PredictionResult
        {
            PredictedPrice = (decimal)Math.Max(0, pred.Score),
            R2 = _r2,
            RMSE = _rmse,
            MAE = _mae,
            ModelName = "FastTree (Gradient Boosting)",
            IsReady = true
        });
    }
}