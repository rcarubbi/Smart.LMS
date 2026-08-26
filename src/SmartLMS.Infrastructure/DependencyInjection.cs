using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions;
using SmartLMS.Infrastructure.Messaging;
using SmartLMS.Infrastructure.Security;

namespace SmartLMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey), "Jwt:SigningKey is required.")
            .ValidateOnStart();

        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName));

        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IEmailSender, MailKitEmailSender>();

        return services;
    }
}
