namespace SmartLMS.Domain.ValueObjects;

/// <summary>
/// A person's display name. Immutable value object; trims input and rejects blank values.
/// </summary>
public readonly record struct PersonName : IEquatable<PersonName>
{
    public const int MaxLength = 200;

    public string Value { get; }

    public PersonName(string value)
    {
        value = value.Trim();
        if (value.Length == 0)
        {
            throw new ArgumentException("Name cannot be blank.", nameof(value));
        }

        if (value.Length > MaxLength)
        {
            throw new ArgumentException($"Name cannot exceed {MaxLength} characters.", nameof(value));
        }

        Value = value;
    }

    public static implicit operator PersonName(string value) => new(value);

    public static implicit operator string(PersonName name) => name.Value;

    public override string ToString() => Value;
}
