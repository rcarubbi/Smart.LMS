# Smart.LMS

An open-source Learning Management System, rebuilt on a modern .NET stack.

## Stack

| Layer | Tech |
|---|---|
| Domain | Pure C# entities, no external dependencies |
| Application | CQRS use cases (`ICommandHandler`/`IQueryHandler` + dispatcher), cross-cutting decorators (logging → FluentValidation → transaction), `Result` pattern |
| Data Access | EF Core 10 (SQL Server), JSON audit trail via `SaveChangesInterceptor` |
| Infrastructure | JWT issuing, BCrypt hashing, MailKit SMTP |
| Presentation | ASP.NET Core minimal APIs, OpenAPI, JWT bearer auth with role policies |
| Background jobs | TickerQ (cron delivery job) + real-time dashboard |
| Frontend | Next.js 16 (App Router), TypeScript, Tailwind v4 |
| Tests | TUnit + NSubstitute (unit), Testcontainers MsSql + WebApplicationFactory (integration) |

## Quick start

```bash
docker compose up -d
```

| Service | URL |
|---|---|
| Web app | http://localhost:3000 |
| API | http://localhost:8080 (OpenAPI at `/openapi/v1.json`) |
| Worker / Jobs dashboard | http://localhost:8081/tickerq (`admin` / `tickerq-admin`) |
| MailPit (SMTP sink) | http://localhost:8025 |

Default admin: `administrator@yourcompany.com` / `smart123`
(the API seeds it plus baseline parameters on startup in Development).

## Local development

```bash
dotnet restore
dotnet build SmartLMS.slnx
dotnet test --solution SmartLMS.slnx        # unit + TestContainers integration

cd web/smartlms-web && npm install && npm run dev   # frontend hot-reload

dotnet run --project src/SmartLMS.Presentation.Api  # API (:5242 dev profile)
dotnet run --project src/SmartLMS.Worker            # worker + dashboard
```

Migrations are applied automatically by the API/worker at startup (Development).
To add one:

```bash
dotnet ef migrations add <Name> -p src/SmartLMS.DataAccess -s src/SmartLMS.Presentation.Api
```

## Solution layout

```
src/
  SmartLMS.Domain/            # entities, pure business rules
  SmartLMS.Application/       # use cases (CQRS), validators, ports, decorators
  SmartLMS.DataAccess/        # EF Core context, mappings, migrations, auditing
  SmartLMS.Infrastructure/    # JWT, BCrypt, MailKit adapters + DB seeder
  SmartLMS.Presentation.Api/  # REST endpoints, auth pipeline
  SmartLMS.Worker/            # TickerQ host, cron jobs, dashboard
tests/
  SmartLMS.UnitTests/
  SmartLMS.IntegrationTests/
web/smartlms-web/             # Next.js frontend
deploy/Dockerfile.web         # web image
```

## Legacy

The original .NET Framework 4.7.2 implementation (ASP.NET MVC 5 + EF6 + Topshelf)
lives on the `master` branch. This branch is a clean-room rewrite.
