namespace MLApp1.Data;

/// <summary>
/// Interface for DATParser
/// </summary>
public interface IDatParser : IDisposable
{
    Task<IEnumerable<string>> EnumeratesImages(int nbImage = int.MaxValue);
}
