using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartLMS.Worker.TickerQPersistence;

/// <summary>Enables `dotnet ef` migrations for the TickerQ persistence context.</summary>
public class SmartLmsTickerDbContextFactory : IDesignTimeDbContextFactory<SmartLmsTickerDbContext>
{
    public SmartLmsTickerDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SMARTLMS_CONNECTION_STRING")
            ?? "Server=localhost,1433;Database=SmartLMS;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<SmartLmsTickerDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new SmartLmsTickerDbContext(options);
    }
}
