using Microsoft.ML;
using MLApp1;
using MLApp1.Extension;
using MLApp1.Labeller;
using System.Collections.Generic;

public class ClassifyWorkflow
{
    public static void ClassifyWorkflowWorkspace(string projectDirectory, string workspaceRelativePath)
    {
        MLContext mlContext = new MLContext();

        IEnumerable<ImageData> LoadImagesFromDirectory(string folder)
        {
            return Directory.EnumerateDirectories(folder)
                     .Where(d => !(d.EndsWith("Dompeux") || d.EndsWith("Bassin")))
                     .SelectMany(folder => Directory.EnumerateFiles(folder))
                     .Union(Directory.EnumerateFiles(folder))
                     .Where(f => Path.GetExtension(f) != ".zip")
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

            File.Move(file.ImagePath,
                Path.Combine(workspaceRelativePath, prediction.PredictedLabel, Path.GetFileName(modelInput.ImagePath)));

            var directory = Path.GetDirectoryName(file.ImagePath);

            if (directory != null &&
                directory != workspaceRelativePath && 
                Directory.EnumerateFiles(directory).Any() == false)
            {
                Directory.Delete(directory, true);
            }
        }
    }

    public static async Task ClassifyNewImagesUsingMlNet(MLAppOptions options, int limit)
    {
        var assets = options.AssetsRelativePath;
        var workspace = options.WorkspacePath;

        var directories = Directory.GetDirectories(assets)
                                   .Shuffle();

        MLContext mlContext = new MLContext();

        IEnumerable<ImageData> LoadImagesFromDirectory(string folder)
        {
            return Directory.EnumerateDirectories(folder)
                     .Where(d => !(d.EndsWith("Dompeux") || d.EndsWith("Bassin")))
                     .SelectMany(folder => Directory.EnumerateFiles(folder))
                     .Union(Directory.EnumerateFiles(folder))
                     .Where(f => Path.GetExtension(f) != ".zip")
                     .Select(file =>
                     {
                         return new ImageData
                         {
                             ImagePath = file,
                             Label = ""
                         };
                     });
        }

        var trainedModel = mlContext.Model.Load(Path.Combine(workspace, "model.zip"), out var schema);

        PredictionEngine<ModelInput, ModelOutput> predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(trainedModel);

        var toClassify = LoadImagesFromDirectory(assets).Take(1000).ToArray();

        var count = 0;

        for (int i = 0; i < directories.Length; i++)
        {
            string? d = directories[i];

            var files = Directory.EnumerateFiles(d).ToArray().Shuffle();

            foreach (var file in files)
            {
                var disposable = new DATParser(file);

                var images = (await disposable.EnumeratesImages(10)).ToArray();

                disposable.Dispose();

                foreach (var image in images)
                {
                    string labelFolder = "";

                    var predication = predictionEngine.Predict(new ModelInput
                    {
                        ImagePath = image,
                    });

                    var label = predication.PredictedLabel;

                    if (string.IsNullOrWhiteSpace(label))
                    {
                        Console.WriteLine("Empty label for image: " + image);
                        break;
                    }
                    labelFolder = Path.Combine(workspace, label);
                    Directory.CreateDirectory(labelFolder);

                    var destination = Path.Combine(labelFolder, Path.GetDirectoryName(file)?.Split('\\').Last() + '.' + Path.GetFileNameWithoutExtension(image) + '.' + count++ + ".jpg");

                    Console.WriteLine($"{image} moving to destination {destination}");
                    File.Move(image, destination, true);

                    if (count >= limit)
                    {
                        return;
                    }
                }
            }
        }
    }
}