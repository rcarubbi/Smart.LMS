namespace SmartLMS.Domain.ValueObjects;

/// <summary>A percentage in the inclusive range 0-100. Immutable value object.</summary>
public readonly record struct Percentual : IEquatable<Percentual>
{
    public int Value { get; }

    public Percentual(int value)
    {
        if (value is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Percentual must be between 0 and 100.");
        }

        Value = value;
    }

    public static implicit operator Percentual(int value) => new(value);

    public static implicit operator int(Percentual value) => value.Value;

    public override string ToString() => $"{Value}%";
}
