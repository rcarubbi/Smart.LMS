using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SmartLMS.Application;
using SmartLMS.Application.Abstractions;
using SmartLMS.DataAccess;
using SmartLMS.DataAccess.Auditing;
using SmartLMS.DataAccess.Seed;
using SmartLMS.Infrastructure;
using SmartLMS.Infrastructure.Security;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Presentation.Api;
using SmartLMS.Presentation.Api.Auth;
using SmartLMS.Presentation.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddDataAccess(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpCurrentUser>();
builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<HttpCurrentUser>());

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();
if (!builder.Environment.IsDevelopment())
{
    var connectionString = builder.Configuration.GetConnectionString("SmartLMS")
        ?? throw new InvalidOperationException("ConnectionStrings:SmartLMS is required outside Development.");
    var sql = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
    var smtpHost = builder.Configuration["Smtp:Host"];
    var publicBaseUrl = builder.Configuration["App:BaseUrl"];
    if (jwtOptions.SigningKey.Length < 32 || jwtOptions.SigningKey.StartsWith("compose-signing-key-", StringComparison.Ordinal) ||
        sql.Password == "yourStrong(!)Password" || string.IsNullOrWhiteSpace(smtpHost) || smtpHost is "localhost" or "mailpit")
    {
        throw new InvalidOperationException("Production requires a private JWT signing key and SQL password.");
    }
    if (string.IsNullOrWhiteSpace(publicBaseUrl) || publicBaseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("App:BaseUrl must point to the deployed web client.");
    }

    var bootstrapEmail = builder.Configuration["BootstrapAdmin:Email"];
    var bootstrapPassword = builder.Configuration["BootstrapAdmin:Password"];
    if (string.IsNullOrWhiteSpace(bootstrapEmail) || string.IsNullOrWhiteSpace(bootstrapPassword) ||
        bootstrapPassword.Length < 12 || bootstrapPassword == DbSeeder.DefaultAdminPassword)
    {
        throw new InvalidOperationException("Production requires a private bootstrap admin email and password (minimum 12 characters).");
    }
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep 'sub' and 'role' claim types as issued
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var subject = context.Principal?.FindFirst("sub")?.Value;
                if (!Guid.TryParse(subject, out var userId))
                {
                    context.Fail("Invalid subject claim.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<ISmartLmsDbContext>();
                if (!await db.Users.AnyAsync(u => u.Id == userId && u.Active, context.HttpContext.RequestAborted))
                {
                    context.Fail("User is inactive.");
                }
            },
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            NameClaimType = "sub",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(nameof(UserRole.Admin)))
    .AddPolicy(Policies.AdminOrTeacher, policy => policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

builder.Services.AddRateLimiter(options => options.AddPolicy("password-reset", context =>
    RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(15),
            QueueLimit = 0,
            AutoReplenishment = true,
        })));

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    if (builder.Environment.IsDevelopment() && !allowedOrigins.Contains("http://localhost:3000", StringComparer.OrdinalIgnoreCase))
    {
        allowedOrigins = [.. allowedOrigins, "http://localhost:3000"];
    }
    if (!builder.Environment.IsDevelopment() &&
        (allowedOrigins.Length == 0 || allowedOrigins.Any(origin => origin.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
            origin.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase))))
    {
        throw new InvalidOperationException("Configure Cors:AllowedOrigins with the deployed web origin.");
    }

    options.AddPolicy("Api", policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "SmartLMS API";
        document.Info.Description = "Learning Management System REST API";
        return Task.CompletedTask;
    });
});

var uploadsPath = builder.Configuration["Uploads:Directory"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "uploads");
var publicUploadsPath = Path.Combine(uploadsPath, "public");
var privateUploadsPath = Path.Combine(uploadsPath, "private");

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<IDbSeeder>().SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("SmartLMS API")
               .WithTheme(ScalarTheme.Kepler)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseCors("Api");
Directory.CreateDirectory(publicUploadsPath);
Directory.CreateDirectory(privateUploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(publicUploadsPath),
    RequestPath = "/uploads",
});
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapCatalogEndpoints();
app.MapLearningEndpoints();
app.MapAssessmentEndpoints();
app.MapUploadEndpoints(publicUploadsPath, privateUploadsPath, uploadsPath);

await app.RunAsync();

/// <summary>Exposes the implicit Program class to WebApplicationFactory-based integration tests.</summary>
public partial class Program;
