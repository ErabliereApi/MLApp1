using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http.Headers;

namespace MLApp1.Extension;
public static class ImageExtension
{
    public static async Task<HttpResponseMessage> PostImageAsync(this Image image, string url, Mode mode, ImageFormat format)
    {
        using var memStream = new MemoryStream();
        image.Save(memStream, format);
        memStream.Position = 0;
        var response = await UploadImageAsync(memStream, Guid.NewGuid().ToString(), url);
        return response;
    }

    public static async Task<HttpResponseMessage> UploadImageAsync(Stream image, string fileName, string url)
    {
        HttpContent fileStreamContent = new StreamContent(image);
        fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "file", FileName = fileName };
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        using (var client = new HttpClient())
        using (var formData = new MultipartFormDataContent())
        {
            formData.Add(fileStreamContent);
            var response = await client.PostAsync(url, formData);
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}

public enum Mode
{
    YCbCr,
    RGB
}