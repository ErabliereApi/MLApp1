using System.Drawing;
using System.Text;

namespace MLApp1.Labeller;

public static class WithoutSpecialCharacterExtension
{
    public static ILabeller WithoutInvalidFileSystemCharacter(this ILabeller labeller)
    {
        return new WithoutSpecialCharacter(labeller);
    } 
}

public class WithoutSpecialCharacter : ILabeller
{
    private ILabeller _labeller;
    private HashSet<char> _invalidChar;

    public WithoutSpecialCharacter(ILabeller labeller)
    {
        _labeller = labeller;
        _invalidChar = new HashSet<char>(Path.GetInvalidPathChars());
        var c = Path.GetInvalidFileNameChars();
        for (int i = 0; i < c.Length; i++)
        {
            _invalidChar.Add(c[i]);
        }
    }

    public async Task<string> GetLabelAsync(Image data)
    {
        var label = await _labeller.GetLabelAsync(data);

        var sb = new StringBuilder();

        for (int i = 0; i < label.Length; i++)
        {
            if (!_invalidChar.Contains(label[i]))
                sb.Append(label[i]);
        }

        return sb.ToString().Trim();
    }
}
