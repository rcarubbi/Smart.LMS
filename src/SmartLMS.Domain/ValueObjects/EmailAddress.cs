using System.Text.RegularExpressions;

namespace SmartLMS.Domain.ValueObjects;

/// <summary>
/// An e-mail address. Immutable value object that validates basic formatting
/// and normalizes to lower-case.
/// </summary>
public readonly record struct EmailAddress : IEquatable<EmailAddress>
{
    public const int MaxLength = 256;

    private static readonly Regex SimplePattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    public EmailAddress(string value)
    {
        value = value.Trim().ToLowerInvariant();
        if (value.Length == 0)
        {
            throw new ArgumentException("E-mail cannot be blank.", nameof(value));
        }

        if (value.Length > MaxLength)
        {
            throw new ArgumentException($"E-mail cannot exceed {MaxLength} characters.", nameof(value));
        }

        if (!SimplePattern.IsMatch(value))
        {
            throw new ArgumentException($"'{value}' is not a valid e-mail address.", nameof(value));
        }

        Value = value;
    }

    public static implicit operator EmailAddress(string value) => new(value);

    public static implicit operator string(EmailAddress email) => email.Value;

    public override string ToString() => Value;
}
