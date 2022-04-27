using MLApp1.Labeller;
using System.Drawing;

public static class ValueMappinLabellerExtension
{
    public static ILabeller WithLabelMapping(this ILabeller labeller, Dictionary<string, string> valueMapping)
    {
        return new ValueMappingLabbeler(labeller, valueMapping);
    }
}

public class ValueMappingLabbeler : ILabeller
{
    private readonly ILabeller _labeller;
    private readonly Dictionary<string, string> _valueMapping;

    public ValueMappingLabbeler(ILabeller labeller, Dictionary<string, string> valueMapping)
    {
        _labeller = labeller;
        _valueMapping = valueMapping;
    }

    public async Task<string> GetLabelAsync(Image image)
    {
        var label = await _labeller.GetLabelAsync(image);

        if (_valueMapping.TryGetValue(label, out var value))
        {
            return value;
        }

        return label;
    }
}