using MLApp1;
using MLApp1.Extension;
using MLApp1.Labeller;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace LabellerImage.Test;

public class ImageCameraLabeller
{
    private ILabeller _labeller = new FromCameraName()
        .WithoutInvalidFileSystemCharacter()
        .WithLabelMapping(new System.Collections.Generic.Dictionary<string, string>
        {
            { "CAML", "CAM1" },
            { "CAME", "CAM3" },
            { "CAMS", "CAM3" }
        });

    [Theory]
    [InlineData("camera1", "CAM1")]
    [InlineData("camera3", "CAM3")]
    [InlineData("camera4", "CAM4")]
    public async Task Camera(string imagefile, string expectedLabel)
    {
        var image = GetImage(imagefile);

        var actuelLabel = await _labeller.GetLabelAsync(image);

        Assert.Equal(expectedLabel, actuelLabel);
    }

    [Fact]
    public async Task PingImage2TextAPI()
    {
        var image = GetImage("ping", "jpg");

        var response = await image.PostImageAsync(
            Config.Image2TextApiUrl, Mode.RGB, ImageFormat.Jpeg);

        var text = await response.Content.ReadAsStringAsync();

        Assert.True(string.IsNullOrWhiteSpace(text));
    }

    [Fact]
    public async Task TestHMI()
    {
        var image = GetImage("hmi", "jpg");

        var response = await image.PostImageAsync(
            Config.Image2TextApiUrl, Mode.RGB, ImageFormat.Jpeg);

        var text = await response.Content.ReadAsStringAsync();

        Assert.Contains("Temperature", text);
    }

    private Image GetImage(string name, string extension = "png")
    {
        name = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Images\\{name}.{extension}");

        return Image.FromFile(name);
    }
}