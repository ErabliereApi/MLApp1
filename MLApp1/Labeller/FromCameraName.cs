using MLApp1.Extension;
using System.Drawing;

namespace MLApp1.Labeller;

public class FromCameraName : ILabeller
{
    public async Task<string> GetLabelAsync(Image data)
    {
        var subImage = new Bitmap(data).Clone(
            new Rectangle(34, 10, 106, 34), data.PixelFormat);

        for (int i = 0; i < subImage.Width; i++)
        {
            for (int j = 0; j < subImage.Height; j++)
            {
                var pixel = subImage.GetPixel(i, j);

                if (pixel.R <= 100)
                {
                    subImage.SetPixel(i, j, Color.White);
                }
                else
                {
                    subImage.SetPixel(i, j, Color.Black);
                }
            }
        }

        var response = await subImage.PostImageAsync(
            Config.Image2TextApiUrl, Mode.RGB, data.RawFormat);

        response.EnsureSuccessStatusCode();

        var s = await response.Content.ReadAsStringAsync();

        return s.Trim();
    }
}
