using Microsoft.ML;

public class ONNXUtil
{
    public static void SaveModelAsONNX(string workspacePath, ITransformer transformer, IDataView inputData, MLContext mlContext)
    {
        var onnxPath = Path.Combine(workspacePath, "onnx_model.onnx");

        using FileStream stream = File.Create(onnxPath);

        mlContext.Model.ConvertToOnnx(transformer, inputData, stream);
    }
}