namespace OutfitPlanner.Domain.ValueObjects;

public class Temperature : ValueObject
{
    public double Value { get; private set; }
    public string Unit { get; private set; } // "C" or "F"

    private Temperature(double value, string unit)
    {
        Value = value;
        Unit = unit;
    }

    public static Temperature Celsius(double value) => new(value, "C");
    public static Temperature Fahrenheit(double value) => new(value, "F");

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Unit;
    }
}
