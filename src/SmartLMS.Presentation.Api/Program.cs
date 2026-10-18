using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
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
    .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(nameof(UserRole.Admin)));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
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

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<IDbSeeder>().SeedAsync();

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("SmartLMS API")
               .WithTheme(ScalarTheme.Kepler)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseCors("Development");
Directory.CreateDirectory(publicUploadsPath);
Directory.CreateDirectory(privateUploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(publicUploadsPath),
    RequestPath = "/uploads",
});
app.UseAuthentication();
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
