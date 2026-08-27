namespace SmartLMS.Domain.ValueObjects;

/// <summary>
/// A non-negative integer (>= 0). Used for ordering, delivery days, and point scores
/// to eliminate primitive obsession and centralize bound validation.
/// </summary>
public readonly record struct NonNegativeInteger : IEquatable<NonNegativeInteger>
{
    public int Value { get; }

    public NonNegativeInteger(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be negative.");
        }

        Value = value;
    }

    public static implicit operator NonNegativeInteger(int value) => new(value);

    public static implicit operator int(NonNegativeInteger value) => value.Value;

    public override string ToString() => Value.ToString();
}
