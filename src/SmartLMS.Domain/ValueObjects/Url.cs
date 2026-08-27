namespace SmartLMS.Domain.ValueObjects;

/// <summary>An absolute HTTP(S) URL. Immutable value object; trims and validates the scheme.</summary>
public readonly record struct Url : IEquatable<Url>
{
    public const int MaxLength = 1000;

    public string Value { get; }

    public Url(string value)
    {
        value = value.Trim();
        if (value.Length == 0)
        {
            throw new ArgumentException("URL cannot be blank.", nameof(value));
        }

        if (value.Length > MaxLength)
        {
            throw new ArgumentException($"URL cannot exceed {MaxLength} characters.", nameof(value));
        }

        var scheme = value.Split(':', 2)[0];
        if (!scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
            && !scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"'{value}' is not a valid http(s) URL.", nameof(value));
        }

        Value = value;
    }

    public static implicit operator Url(string value) => new(value);

    public static implicit operator string(Url url) => url.Value;

    public override string ToString() => Value;
}
