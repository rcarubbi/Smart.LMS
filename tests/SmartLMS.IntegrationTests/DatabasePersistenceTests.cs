using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartLMS.Application.Abstractions;
using SmartLMS.DataAccess;
using SmartLMS.DataAccess.Auditing;
using SmartLMS.DataAccess.Seed;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Infrastructure.Security;
using Testcontainers.MsSql;

namespace SmartLMS.IntegrationTests;

/// <summary>
/// Boots a disposable SQL Server container, applies the EF Core migrations
/// and exercises persistence + auditing roundtrips against real SQL Server.
/// </summary>
public class DatabasePersistenceTests
{
    private SmartLmsDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<SmartLmsDbContext>()
            .UseSqlServer(connectionString)
            .AddInterceptors(new AuditSaveChangesInterceptor(new TestCurrentUser()))
            .Options;

        return new SmartLmsDbContext(options);
    }

    [Test]
    [ClassDataSource<TestSqlContainer>(Shared = SharedType.PerTestSession)]
    public async Task Migrations_apply_seed_and_audit_roundtrip_succeeds(TestSqlContainer db)
    {
        var connectionString = db.ConnectionStringForDatabase("persistence");

        await using (var context = CreateContext(connectionString))
        {
            await context.Database.MigrateAsync();

            // Seed creates admin + baseline parameters idempotently.
            var seeder = new DbSeeder(context, new BCryptPasswordHasher(), NullLogger<DbSeeder>.Instance);
            await seeder.SeedAsync();

            var parameters = await context.Parameters.ToListAsync();
            using (Assert.Multiple())
            {
                await Assert.That(parameters).Count().IsGreaterThanOrEqualTo(3);
                await Assert.That(parameters.Any(p => p.Key == Domain.Entities.Parameter.AppNameKey)).IsTrue();
            }
        }

        Guid courseId;

        await using (var context = CreateContext(connectionString))
        {
            var area = new KnowledgeArea("Engineering", 1);
            var subject = new Subject("Software Architecture", 1, area.Id);
            var course = new Course("Clean Architecture", 1, subject.Id);

            context.AddRange(area, subject, course);
            await context.SaveChangesAsync();

            courseId = course.Id;
            await Assert.That(courseId).IsNotEqualTo(Guid.Empty);
        }

        await using (var context = CreateContext(connectionString))
        {
            // Auditing recorded inserts.
            var logs = await context.AuditLogs.Where(l => l.EntityId == courseId).ToListAsync();
            await Assert.That(logs.Select(l => l.Type)).IsEquivalentTo(["Course"]);

            // Modification produces old/new state pair.
            var course = await context.Courses.SingleAsync(c => c.Id == courseId);
            course.Rename("Clean Architecture v2");
            await context.SaveChangesAsync();

            var updateLogs = await context.AuditLogs
                .Where(l => l.EntityId == courseId && l.Type == "Course")
                .OrderBy(l => l.DateTime)
                .ToListAsync();

            await Assert.That(updateLogs.Count).IsEqualTo(2);
            await Assert.That(updateLogs[1].OldState).Contains("Clean Architecture");
            await Assert.That(updateLogs[1].NewState).Contains("Clean Architecture v2");

            var persisted = await context.Courses.SingleAsync(c => c.Id == courseId);
            await Assert.That(persisted.Name).IsEqualTo("Clean Architecture v2");
        }
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid? UserId => null;

        public UserRole? Role => null;
    }
}
