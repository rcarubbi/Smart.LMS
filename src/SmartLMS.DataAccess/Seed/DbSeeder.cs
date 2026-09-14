using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartLMS.Application.Abstractions;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.DataAccess.Seed;

/// <summary>Applies migrations and creates the default admin + baseline parameters.</summary>
public sealed class DbSeeder(
    SmartLmsDbContext context,
    IPasswordHasher passwordHasher,
    ILogger<DbSeeder> logger) : IDbSeeder
{
    public const string DefaultAdminEmail = "administrator@yourcompany.com";
    public const string DefaultAdminPassword = "smart123";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        var admin = await context.Set<Admin>().FirstOrDefaultAsync(u => u.Email == DefaultAdminEmail, cancellationToken);
        if (admin is null)
        {
            var newAdmin = new Admin("Administrator", DefaultAdminEmail);
            newAdmin.UpdatePasswordHash(passwordHasher.Hash(DefaultAdminPassword));
            context.Set<Admin>().Add(newAdmin);

            logger.LogInformation("Default admin user created ({Email})", DefaultAdminEmail);
        }

        await SeedParameterIfMissingAsync(Parameter.AppNameKey, "Smart LMS", cancellationToken);
        await SeedParameterIfMissingAsync(Parameter.BaseUrlKey, "http://localhost:3000", cancellationToken);
        await SeedParameterIfMissingAsync(Parameter.FileStorageKey, "/app/storage", cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedParameterIfMissingAsync(
        string key,
        string value,
        CancellationToken cancellationToken)
    {
        if (!await context.Parameters.AnyAsync(p => p.Key == key, cancellationToken))
        {
            context.Parameters.Add(new Parameter { Key = key, Value = value });
        }
    }
}
