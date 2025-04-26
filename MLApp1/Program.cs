using MLApp1;
using MLApp1.Data;

Console.WriteLine("COMPlus_gcAllowVeryLargeObjects: " + Environment.GetEnvironmentVariable("COMPlus_gcAllowVeryLargeObjects"));

var options = new MLAppOptions
{
    ProjectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../")),
};

options.WorkspacePath = Path.Combine(options.ProjectDirectory, "workspace", "erabliere");
options.AssetsRelativePath = Path.Combine(@"D:\");
options.Temp = Path.Combine(options.ProjectDirectory, "Temp");

Directory.CreateDirectory(options.WorkspacePath);

if (Directory.Exists(options.Temp))
{
    Directory.Delete(options.Temp, true);
}

while (true)
{
    Console.WriteLine("Welcome to MLApp1.");
    Console.WriteLine("1. Prepare folder");
    Console.WriteLine("2. Train a model");
    Console.WriteLine("3. Classify images");
    Console.WriteLine("4. Classify image folder using image2text api");
    Console.WriteLine("5. Classify image using ML.Net");
    Console.WriteLine("6. Edit options");
    Console.WriteLine("0. Quit");
    Console.Write(">>> ");

    var c = Console.ReadLine()?.Trim();

    try
    {
        switch (c)
        {
            case "1":
                await PrepareFolder.PreparerFolderAsync(options.AssetsRelativePath, options.WorkspacePath, 5000);
                break;
            case "2":
                TrainWorkflow.TrainWorkflowAsync(options.WorkspacePath);
                break;
            case "3":
                ClassifyWorkflow.ClassifyWorkflowWorkspace(options.ProjectDirectory, options.WorkspacePath);
                break;
            case "4":
                await ClassifyFolder.ClassifyFolderUsingImge2Text(options.AssetsRelativePath, options.WorkspacePath);
                break;
            case "5":
                await ClassifyWorkflow.ClassifyNewImagesUsingMlNet(options, 1000);
                break;
            case "6":
                EditOptions(options);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid input");
                break;
        }
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e);
    }
}

void EditOptions(MLAppOptions options)
{
    while (true)
    {
        Console.WriteLine("1. ProjectDirectory: " + options.ProjectDirectory);
        Console.WriteLine("2. WorkspacePath: " + options.WorkspacePath);
        Console.WriteLine("3. AssertsRelativePath: " + options.AssetsRelativePath);
        Console.WriteLine("0. Back to main menu");

        Console.WriteLine("Choose a options to edit");
        Console.Write(">>> ");

        var o = Console.ReadLine()?.Trim();

        switch (o)
        {
            case "1":
                Console.WriteLine("Enter new ProjectDirectory value");
                Console.Write(">>> ");
                options.ProjectDirectory = Console.ReadLine() ?? "";
                break;
            case "2":
                Console.WriteLine("Enter new WorkspacePath value");
                Console.Write(">>> ");
                options.WorkspacePath = Console.ReadLine() ?? "";
                break;
            case "3":
                Console.WriteLine("Enter new AssetsRelativePath value");
                Console.Write(">>> ");
                options.AssetsRelativePath = Console.ReadLine() ?? "";
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Unkown choice");
                break;
        }
    }
}