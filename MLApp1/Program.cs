using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;
using Microsoft.ML.Vision;
using MLApp1;
using MLApp1.Data;

Console.WriteLine(Environment.GetEnvironmentVariable("COMPlus_gcAllowVeryLargeObjects"));

var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
var workspaceRelativePath = Path.Combine(projectDirectory, "workspace");
var assetsRelativePath = Path.Combine("V:\\");

Directory.CreateDirectory(workspaceRelativePath);

if (Directory.Exists(Path.Combine(projectDirectory, "Temp")))
{
    Directory.Delete(Path.Combine(projectDirectory, "Temp"), true);
}

await PrepareFolder.PreparerFolderAsync(assetsRelativePath, workspaceRelativePath, 32000);

MLContext mlContext = new MLContext();

IEnumerable<ImageData> LoadImagesFromDirectory(string folder)
{
    return Directory.EnumerateDirectories(folder)
             .SelectMany(folder => Directory.EnumerateFiles(folder))
             .Select(file =>
             {
                 var label = Path.GetDirectoryName(file)?.Split('\\').Last();

                 return new ImageData
                 {
                     ImagePath = file,
                     Label = label ?? throw new ArgumentNullException(nameof(label)),
                 };
             });
    //var queue = new ConcurrentQueue<string>();
    //var t = PrepareFolder.PreparerFolderAsync(assetsRelativePath, workspaceRelativePath, 1000, queue);
    //while (!t.IsCompleted)
    //{
    //    if (queue.TryDequeue(out var file))
    //    {
    //        yield return new ImageData
    //        {
    //            ImagePath = file,
    //            Label = Path.GetDirectoryName(file) ?? "",
    //        };
    //    }
    //}
}

IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: workspaceRelativePath);

IDataView imageData = mlContext.Data.LoadFromEnumerable(images);

IDataView shuffledData = mlContext.Data.ShuffleRows(imageData);

var preprocessingPipeline = mlContext.Transforms.Conversion.MapValueToKey(
        inputColumnName: "Label",
        outputColumnName: "LabelAsKey")
    .Append(mlContext.Transforms.LoadRawImageBytes(
        outputColumnName: "Image",
        imageFolder: workspaceRelativePath,
        inputColumnName: "ImagePath"));

IDataView preProcessedData = preprocessingPipeline
                    .Fit(imageData)
                    .Transform(imageData);

TrainTestData trainSplit = mlContext.Data.TrainTestSplit(data: preProcessedData, testFraction: 0.3);
TrainTestData validationTestSplit = mlContext.Data.TrainTestSplit(trainSplit.TestSet);

IDataView trainSet = trainSplit.TrainSet;
IDataView validationSet = validationTestSplit.TrainSet;
IDataView testSet = validationTestSplit.TestSet;

void PrintPreviewRows(IDataView trainSetDV,
            IDataView testSetDV)

{
    var trainSet = mlContext.Data.CreateEnumerable<ModelInput>(trainSetDV, true);

    Console.WriteLine($"The data in the Train split.");
    foreach (var row in trainSet)
        Console.WriteLine($"{row.ImagePath}, {row.Label}, {row.LabelAsKey}");

    var testSet = mlContext.Data.CreateEnumerable<ModelInput>(testSetDV, true);

    Console.WriteLine($"\nThe data in the Test split.");
    foreach (var row in testSet)
        Console.WriteLine($"{row.ImagePath}, {row.Label}, {row.LabelAsKey}");
}

PrintPreviewRows(trainSet, testSet);

var vSet = mlContext.Data.CreateEnumerable<ModelInput>(validationSet, true);

Console.WriteLine($"The data in the Validation split.");
foreach (var row in vSet)
    Console.WriteLine($"{row.ImagePath}, {row.Label}, {row.LabelAsKey}");

var classifierOptions = new ImageClassificationTrainer.Options()
{
    FeatureColumnName = "Image",
    LabelColumnName = "LabelAsKey",
    ValidationSet = validationSet,
    Arch = ImageClassificationTrainer.Architecture.ResnetV2101,
    MetricsCallback = (metrics) => Console.WriteLine(metrics),
    TestOnTrainSet = false,
    ReuseTrainSetBottleneckCachedValues = true,
    ReuseValidationSetBottleneckCachedValues = true,
};

var trainingPipeline = mlContext.MulticlassClassification.Trainers.ImageClassification(classifierOptions)
    .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

ITransformer trainedModel = trainingPipeline.Fit(trainSet);

static void OutputPrediction(ModelOutput prediction)
{
    string imageName = Path.GetFileName(prediction.ImagePath);
    Console.WriteLine($"Image: {imageName} | Actual Value: {prediction.Label} | Predicted Value: {prediction.PredictedLabel}");
}

void ClassifySingleImage(MLContext mlContext, IDataView data, ITransformer trainedModel)
{
    PredictionEngine<ModelInput, ModelOutput> predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(trainedModel);

    ModelInput image = mlContext.Data.CreateEnumerable<ModelInput>(data, reuseRowObject: true).First();

    ModelOutput prediction = predictionEngine.Predict(image);

    Console.WriteLine("Classifying single image");
    OutputPrediction(prediction);
}

ClassifySingleImage(mlContext, testSet, trainedModel);

void ClassifyImages(MLContext mlContext, IDataView data, ITransformer trainedModel)
{
    IDataView predictionData = trainedModel.Transform(data);

    IEnumerable<ModelOutput> predictions = mlContext.Data.CreateEnumerable<ModelOutput>(predictionData, reuseRowObject: true).Take(10);

    Console.WriteLine("Classifying multiple images");
    foreach (var prediction in predictions)
    {
        OutputPrediction(prediction);
    }
}

ClassifyImages(mlContext, testSet, trainedModel);