# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props NuGet.config ./
COPY SmartLMS.slnx ./
COPY src/SmartLMS.Domain/SmartLMS.Domain.csproj src/SmartLMS.Domain/
COPY src/SmartLMS.Application/SmartLMS.Application.csproj src/SmartLMS.Application/
COPY src/SmartLMS.DataAccess/SmartLMS.DataAccess.csproj src/SmartLMS.DataAccess/
COPY src/SmartLMS.Infrastructure/SmartLMS.Infrastructure.csproj src/SmartLMS.Infrastructure/
COPY src/SmartLMS.Presentation.Api/SmartLMS.Presentation.Api.csproj src/SmartLMS.Presentation.Api/
COPY src/SmartLMS.Worker/SmartLMS.Worker.csproj src/SmartLMS.Worker/
COPY tests/SmartLMS.UnitTests/SmartLMS.UnitTests.csproj tests/SmartLMS.UnitTests/
COPY tests/SmartLMS.IntegrationTests/SmartLMS.IntegrationTests.csproj tests/SmartLMS.IntegrationTests/

RUN dotnet restore SmartLMS.slnx

COPY . .
RUN dotnet publish src/SmartLMS.Worker -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8081
EXPOSE 8081

ENTRYPOINT ["dotnet", "SmartLMS.Worker.dll"]
