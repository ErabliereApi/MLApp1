using Microsoft.ML;
using MLApp1;

public class ClassifyWorkflow
{
    public static void ClassifyWorkflowWorkspace()
    {
        var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
        var workspaceRelativePath = Path.Combine(projectDirectory, "workspace");

        MLContext mlContext = new MLContext();

        IEnumerable<ImageData> LoadImagesFromDirectory(string folder)
        {
            return Directory.EnumerateDirectories(folder)
                     .Where(d => !(d.EndsWith("Dompeux") || d.EndsWith("Bassin")))
                     .SelectMany(folder => Directory.EnumerateFiles(folder))
                     .Union(Directory.EnumerateFiles(folder))
                     .Select(file =>
                     {
                         return new ImageData
                         {
                             ImagePath = file,
                             Label = ""
                         };
                     });
        }

        var trainedModel = mlContext.Model.Load(Path.Combine(workspaceRelativePath, "model.zip"), out var schema);

        PredictionEngine<ModelInput, ModelOutput> predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(trainedModel);

        var toClassify = LoadImagesFromDirectory(workspaceRelativePath).ToArray();

        for (int i = 0; i < toClassify.Length; i++)
        {
            var file = toClassify[i];

            var modelInput = new ModelInput
            {
                Image = File.ReadAllBytes(file.ImagePath),
                Label = "",
                ImagePath = file.ImagePath,
                LabelAsKey = default
            };

            var prediction = predictionEngine.Predict(modelInput);

            Console.WriteLine(prediction.PredictedLabel + " " + prediction.ImagePath);
        }
    }
}