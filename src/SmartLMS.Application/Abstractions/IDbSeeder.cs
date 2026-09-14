namespace SmartLMS.Application.Abstractions;

/// <summary>
/// Bootstraps the database: applies migrations, creates the default admin
/// and baseline parameters. Implemented in DataAccess; consumed by hosts.
/// </summary>
public interface IDbSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
