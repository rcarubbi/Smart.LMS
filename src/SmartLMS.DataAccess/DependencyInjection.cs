using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions;
using SmartLMS.DataAccess.Auditing;
using SmartLMS.DataAccess.Repositories;
using SmartLMS.Domain.Entities.Assessments;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.DataAccess;

public static class DependencyInjection
{
    public const string ConnectionStringName = "SmartLMS";

    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddDbContext<SmartLmsDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddScoped<ISmartLmsDbContext>(sp => sp.GetRequiredService<SmartLmsDbContext>());

        RegisterRepository<User>(services);
        RegisterRepository<Student>(services);
        RegisterRepository<Teacher>(services);
        RegisterRepository<Admin>(services);
        RegisterRepository<KnowledgeArea>(services);
        RegisterRepository<Subject>(services);
        RegisterRepository<Course>(services);
        RegisterRepository<Class>(services);
        RegisterRepository<SupportFile>(services);
        RegisterRepository<Classroom>(services);
        RegisterRepository<DeliveryPlan>(services);
        RegisterRepository<Notice>(services);
        RegisterRepository<Comment>(services);
        RegisterRepository<Assessment>(services);

        return services;
    }

    private static void RegisterRepository<T>(IServiceCollection services) where T : class
    {
        services.AddScoped<IRepositoryBase<T>, EfRepository<T>>();
        services.AddScoped<IReadRepositoryBase<T>, EfRepository<T>>();
    }
}
