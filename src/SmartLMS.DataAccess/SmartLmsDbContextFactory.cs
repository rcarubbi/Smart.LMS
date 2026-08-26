using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartLMS.DataAccess;

/// <summary>
/// Enables `dotnet ef` tooling without booting the host.
/// Connection string resolution order: SMARTLMS_CONNECTION_STRING env var,
/// then the Docker Compose SQL Server endpoint.
/// </summary>
public class SmartLmsDbContextFactory : IDesignTimeDbContextFactory<SmartLmsDbContext>
{
    public SmartLmsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SMARTLMS_CONNECTION_STRING")
            ?? "Server=localhost,1433;Database=SmartLMS;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<SmartLmsDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new SmartLmsDbContext(options);
    }
}
