using System.Text.RegularExpressions;

namespace OutfitPlanner.Domain.ValueObjects;

public class Color : ValueObject
{
    public string HexCode { get; private set; }
    public string Name { get; private set; }

    private Color(string hexCode, string name)
    {
        if (!Regex.IsMatch(hexCode, "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$"))
        {
            throw new ArgumentException("Invalid Hex Code");
        }
        HexCode = hexCode;
        Name = name;
    }

    public static Color From(string hexCode, string name)
    {
        return new Color(hexCode, name);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return HexCode;
    }
}
