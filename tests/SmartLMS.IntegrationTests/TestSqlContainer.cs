using Testcontainers.MsSql;
using TUnit.Core.Interfaces;

namespace SmartLMS.IntegrationTests;

/// <summary>
/// One SQL Server container for the whole test session, shared via a TUnit
/// <c>[ClassDataSource(Shared = PerTestSession)]</c> fixture. Each test gets its
/// own database on the shared server so tests stay isolated without starting a
/// container per test method.
/// </summary>
public sealed class TestSqlContainer : IAsyncInitializer, IAsyncDisposable
{
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();

    public Task InitializeAsync() => _container.StartAsync();

    public string ConnectionStringForDatabase(string database) =>
        _container.GetConnectionString().Replace("Database=master", $"Database={database}", StringComparison.Ordinal);

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
