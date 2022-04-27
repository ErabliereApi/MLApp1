namespace MLApp1;
public class ModelInput
{
    public ModelInput()
    {
        Image = Array.Empty<byte>();
        ImagePath = "";
        Label = "";
    }

    public byte[] Image { get; set; }

    public UInt32 LabelAsKey { get; set; }

    public string ImagePath { get; set; }

    public string Label { get; set; }
}