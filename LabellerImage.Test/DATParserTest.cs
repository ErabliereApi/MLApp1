using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LabellerImage.Test;
public class DATParserTest : IDisposable
{
    public void Dispose()
    {
        Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\Temp", true);
    }

    [Fact]
    public async Task TestGetCountImage()
    {
        var parser = new DATParser(AppDomain.CurrentDomain.BaseDirectory + "\\Videos\\file0000.dat");

        var images = await parser.EnumeratesImages();

        Assert.True(images.Count() > 1);
    }
}
