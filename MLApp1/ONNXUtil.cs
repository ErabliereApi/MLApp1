using Microsoft.ML;
using MLApp1;

public class ONNXUtil
{
    public static void SaveModelAsONNX(string workspacePath, IDataView inputData, MLContext mlContext)
    {
        var onnxPath = Path.Combine(workspacePath, "onnx_model.onnx");

        using FileStream stream = File.Create(onnxPath);

        var transformer = mlContext.Model.Load(Path.Combine(workspacePath, "model.zip"), out var schema);

        mlContext.Model.ConvertToOnnx(transformer, inputData, stream);
    }
}