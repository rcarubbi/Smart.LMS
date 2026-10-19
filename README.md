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

`docker-compose.yml` is a **development-only** convenience stack with weak defaults. Do not reuse these credentials in deployment.

## Deployment configuration

API and worker require production secrets; both reject the Compose development JWT/SQL values. Configure secrets and public URLs before building:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export MSSQL_PID=Standard # if using the bundled SQL Server container; requires the matching license
export MSSQL_SA_PASSWORD='<strong unique SQL password>'
export JWT_SIGNING_KEY='<random secret of at least 32 characters>'
export TICKERQ_DASHBOARD_USERNAME='<private dashboard user>'
export TICKERQ_DASHBOARD_PASSWORD='<strong unique dashboard password>'
export BOOTSTRAP_ADMIN_EMAIL='admin@example.com'
export BOOTSTRAP_ADMIN_PASSWORD='<unique password of at least 12 characters>'
export CORS_ALLOWED_ORIGIN='https://lms.example.com'
export APP_BASE_URL='https://lms.example.com'
export NEXT_PUBLIC_API_URL='https://api.example.com'
export SMTP_HOST='smtp.example.com'
docker compose up -d --build
```

`NEXT_PUBLIC_API_URL` is embedded into the browser bundle at build time; rebuild the web image when it changes. `Cors__AllowedOrigins__0` must match the deployed web origin. The API applies main database migrations and creates the configured bootstrap admin on startup; the worker applies both application and TickerQ migrations. EF Core serializes concurrent migration attempts. Configure SMTP via `SMTP_HOST`, `SMTP_PORT`, `SMTP_FROM_ADDRESS`, and optional credentials. `App__BaseUrl` is the public web URL used in password-reset links and seeds the `BaseUrl` parameter on a new database.

Outbox email delivery is **at-least-once**: a crash after SMTP accepts a message but before the database marks it processed can resend that message after its lease expires. Failures retry after 1, 2, 4, then 8 minutes; after five failed attempts, admins can inspect and manually retry from `/admin/outbox`.

## Local development

```bash
dotnet restore
dotnet build SmartLMS.slnx
dotnet test --solution SmartLMS.slnx        # unit + TestContainers integration

cd web/smartlms-web && npm install && npm run dev   # frontend hot-reload

dotnet run --project src/SmartLMS.Presentation.Api  # API (:5242 dev profile)
dotnet run --project src/SmartLMS.Worker            # worker + dashboard
```

The API applies main database migrations and seeds local development data at startup. The worker applies TickerQ migrations.
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
