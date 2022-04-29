using MLApp1.Labeller;
using System.Drawing;

namespace MLApp1;
public class ClassifyFolder
{
    public static async Task ClassifyFolderUsingImge2Text(string assets, string workspace)
    {
        var files = Directory.EnumerateFiles(assets);

        var labeller = new FromCameraName()
       .WithoutInvalidFileSystemCharacter()
       .WithLabelMapping(new Dictionary<string, string>
       {
            { "CAML", "CAM1" },
            { "CAME", "CAM3" },
            { "CAMS", "CAM3" },
            { "CAME2", "CAM2"  }

       }).WithLabelMapping(new Dictionary<string, string>
       {
            { "CAM1", "Dompeux" },
            { "CAM2", "Bassin" },
            { "CAM3", "Bassin" },
            { "CAM4", "Bassin" },
       });

        await Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = 2 }, async (file, cancellationToken) =>
        {
            string label = "";

            using (var image = Image.FromFile(file))
            {
                label = await labeller.GetLabelAsync(image);
            }

            var currentLabel = Path.GetDirectoryName(file)?.Split('\\').Last();

            if (currentLabel != label)
            {
                var dest = Path.Combine(workspace, label, Path.GetFileName(file));
                var destDir = Path.GetDirectoryName(dest);
                if (Directory.Exists(destDir))
                {
                    Console.WriteLine($"Moving {file} to {destDir}");
                    File.Move(file, dest);
                }
                else
                {
                    Console.WriteLine($"Not moving image {file} because de destination dosen't exist {dest}.");
                }
            }
        });
    }
}
