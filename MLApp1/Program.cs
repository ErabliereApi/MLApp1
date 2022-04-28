using MLApp1;

do
{
    Console.WriteLine("Welcome to MLApp1.");
    Console.WriteLine("1. Train a model");
    Console.WriteLine("2. Classify images");
    Console.WriteLine("0. Quit");
    Console.Write(">>> ");

    var c = Console.ReadLine()?.Trim();

    try
    {
        switch (c)
        {
            case "1":
                await TrainWorkflow.TrainWorkflowAsync();
                break;
            case "2":
                ClassifyWorkflow.ClassifyWorkflowWorkspace();
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
    
} while (true);


