using MLApp1.Extension;
using MLApp1.Labeller;
using System.Drawing;

namespace MLApp1.Data;
public class PrepareFolder
{
    public static async Task PreparerFolderAsync(string assetsRelativePath, string workspaceRelativePath, int limit, System.Collections.Concurrent.ConcurrentQueue<string>? queue = null)
    {
        var directories = Directory.GetDirectories(assetsRelativePath)
                                   .Shuffle();

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
            { "CAME", "CAM3" },
            { "CAMS", "CAM3" }

        }).WithLabelMapping(new Dictionary<string, string>
        {
            { "CAM1", "Dompeux" },
            { "CAM2", "Bassin" },
            { "CAM3", "Bassin" },
            { "CAM4", "Bassin" },
        });

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

                    var destination = Path.Combine(labelFolder, Path.GetDirectoryName(file)?.Split('\\').Last() + '.' + Path.GetFileNameWithoutExtension(image) + '.' + count + ".jpg");

                    Console.WriteLine($"Moving to: {destination}");
                    File.Move(image, destination, true);

                    queue?.Enqueue(destination);

                    count++;

                    if (count >= limit)
                    {
                        return;
                    }                    
                }
            }
        }
    }
}