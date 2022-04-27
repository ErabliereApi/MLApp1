using System.Drawing;

namespace MLApp1.Labeller;

public interface ILabeller
{
    Task<string> GetLabelAsync(Image data);
}
