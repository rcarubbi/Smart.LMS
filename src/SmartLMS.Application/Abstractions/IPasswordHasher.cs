namespace SmartLMS.Application.Abstractions;

/// <summary>Port for password hashing (implemented with BCrypt in Infrastructure).</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
