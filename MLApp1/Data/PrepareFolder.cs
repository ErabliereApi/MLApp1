using MLApp1.Extension;
using MLApp1.Labeller;
using System.Collections.Concurrent;
using System.Drawing;

namespace MLApp1.Data;
public class PrepareFolder
{
    /// <summary>
    /// Extract images for video file and do a first classification using image2text api
    /// </summary>
    /// <param name="assetsRelativePath">Location of the images.</param>
    /// <param name="workspaceRelativePath">Destination folder of the extracted images.</param>
    /// <param name="limit">Maximum number of images in workspace filter.</param>
    /// <param name="queue">Optionnal, if a initialize ConsurrentQueue is pass, once an image is save in the workspace folder, it will enqueue the path of the file.</param>
    /// <returns></returns>
    public static async Task PreparerFolderAsync(string assetsRelativePath, string workspaceRelativePath, int limit, ConcurrentQueue<string>? queue = null)
    {
        var directories = Directory.GetDirectories(assetsRelativePath).Shuffle();

        var count = Directory.GetDirectories(workspaceRelativePath)
                             .Where(d => d.EndsWith("Dompeux") || d.EndsWith("Bassin"))
                             .Select(d => Directory.GetFiles(d).Length)
                             .DefaultIfEmpty(0).Sum();

        if (count >= limit)
        {
            return;
        }

        var labeller = new FromCameraName()
        .WithoutInvalidFileSystemCharacter()
        .WithLabelMapping(new Dictionary<string, string>
        {
            { "CAML", "CAM1" },
            { "CAME2", "CAM2" },
            { "CAME", "CAM3" },
            { "CAMS", "CAM3" },
            { "CAH", "CAM4" },
            { "SAML", "CAM1" },
            { "AML", "CAM1" },
            { "CAAM3", "CAM3" },
            { "CAMH", "CAM4" },
            { "CAM.3", "CAM3" }

        }).WithLabelMapping(new Dictionary<string, string>
        {
            { "CAM1", "Dompeux" },
            { "CAM2", "Bassin" },
            { "CAM3", "Separateur" },
            { "CAM4", "Bassin" },
        });

        var imageProcessed = 0;
        var folderProcessed = 0;

        for (int i = 0; i < directories.Length; i++)
        {
            string? d = directories[i];

            Console.WriteLine($"Processing folder {d}");

            var files = Directory.EnumerateFiles(d).ToArray().Shuffle();

            foreach (var file in files)
            {
                IDatParser datParser = new DatParserV2(file);

                var images = (await datParser.EnumeratesImages(10)).ToArray();

                datParser.Dispose();

                foreach (var image in images)
                {
                    Console.WriteLine($"Processing images: {imageProcessed}");

                    string labelFolder = "";

                    using (var fs = new FileStream(image, FileMode.Open, FileAccess.Read))
                    {
                        using (var imageData = Image.FromStream(fs, useEmbeddedColorManagement: false, validateImageData: false))
                        {
                            var label = await labeller.GetLabelAsync(imageData);
                            if (string.IsNullOrWhiteSpace(label))
                            {
                                Console.WriteLine("Enpty label for image: " + image);
                            }
                            labelFolder = Path.Combine(workspaceRelativePath, label);
                            Directory.CreateDirectory(labelFolder);
                        }
                    }

                    var destination = Path.Combine(labelFolder, $"{DateTime.Now.Year}." + Path.GetDirectoryName(file)?.Split('\\').Last() + '.' + Path.GetFileNameWithoutExtension(image) + '.' + count + ".jpg");

                    Console.WriteLine($"Moving to: {destination}");
                    File.Move(image, destination, true);

                    queue?.Enqueue(destination);

                    count++;
                    imageProcessed++;

                    if (count >= limit)
                    {
                        return;
                    }
                }
            }

            folderProcessed++;

            Console.WriteLine($"Progress: {Math.Round(folderProcessed / (double)directories.Length, 2)} $");
        }
    }
}