using Ardalis.Specification.EntityFrameworkCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Authorization;
using SmartLMS.Domain.Specifications;
using ErrorOr;

namespace SmartLMS.Application.Learning.History;

public sealed record RecordClassAccessInput(Guid ClassId, int Percentual, decimal WatchedSeconds);

public sealed record RecordClassAccessCommand(RecordClassAccessInput Input) : ICommand;

internal sealed class RecordClassAccessValidator : AbstractValidator<RecordClassAccessCommand>
{
    public RecordClassAccessValidator()
    {
        RuleFor(c => c.Input.ClassId).NotEmpty();
        RuleFor(c => c.Input.Percentual).InclusiveBetween(0, 100);
        RuleFor(c => c.Input.WatchedSeconds).GreaterThanOrEqualTo(0);
    }
}

internal sealed class RecordClassAccessHandler(
    ISmartLmsDbContext context,
    ICurrentUser currentUser,
    ClassAccessPolicy accessPolicy,
    TimeProvider timeProvider)
    : ICommandHandler<RecordClassAccessCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(RecordClassAccessCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");
        }

        if (!await accessPolicy.CanAccessClassAsync(command.Input.ClassId, cancellationToken))
        {
            return Error.NotFound("Classes.NotFound", "Class not found.");
        }

        var existing = await context.ClassAccesses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.ClassId == command.Input.ClassId, cancellationToken);

        var accessTime = timeProvider.GetUtcNow().UtcDateTime;

        Domain.Entities.History.ClassAccess? added = null;
        if (existing is null)
        {
            added = new Domain.Entities.History.ClassAccess(
                userId,
                command.Input.ClassId,
                command.Input.Percentual,
                command.Input.WatchedSeconds,
                accessTime);
            context.ClassAccesses.Add(added);
        }
        else
        {
            existing.UpdateProgress(command.Input.Percentual, command.Input.WatchedSeconds, accessTime);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Two heartbeats raced on the unique (UserId, ClassId): detach our
            // loser insert, reload the winner and apply monotonic progress
            // instead of surfacing a 500.
            if (added is not null && context is DbContext db)
            {
                db.Entry(added).State = EntityState.Detached;
            }

            var winner = await context.ClassAccesses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.ClassId == command.Input.ClassId, cancellationToken);
            if (winner is null)
            {
                throw;
            }

            winner.UpdateProgress(command.Input.Percentual, command.Input.WatchedSeconds, accessTime);
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success;
    }
}

public sealed record RecordFileAccessInput(Guid FileId);

public sealed record RecordFileAccessCommand(RecordFileAccessInput Input) : ICommand;

internal sealed class RecordFileAccessHandler(
    ISmartLmsDbContext context,
    ICurrentUser currentUser,
    ClassAccessPolicy accessPolicy,
    TimeProvider timeProvider)
    : ICommandHandler<RecordFileAccessCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(RecordFileAccessCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");
        }

        if (!await accessPolicy.CanAccessFileAsync(command.Input.FileId, cancellationToken))
        {
            return Error.NotFound("Files.NotFound", "File not found.");
        }

        context.FileAccesses.Add(new Domain.Entities.History.FileAccess(
            userId,
            command.Input.FileId,
            timeProvider.GetUtcNow().UtcDateTime));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record MyProgressItem(
    Guid ClassId,
    string ClassName,
    string CourseName,
    int Percentual,
    decimal WatchedSeconds,
    DateTime LastAccessAtUtc);

public sealed record MyProgressQuery(Guid? ClassId = null, int? Page = null, int? PageSize = null)
    : IQuery<Paged<MyProgressItem>>;

internal sealed class MyProgressHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<MyProgressQuery, Paged<MyProgressItem>>
{
    public async ValueTask<ErrorOr<Paged<MyProgressItem>>> Handle(
        MyProgressQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");
        }

        var accessSpec = new ClassAccessesByUserSpec(userId, query.ClassId);
        var items = await SpecificationEvaluator.Default
            .GetQuery(context.ClassAccesses.AsNoTracking(), accessSpec)
            .OrderByDescending(a => a.AccessDateTime)
            .Select(a => new MyProgressItem(
                a.ClassId,
                a.Class.Name,
                a.Class.Course.Name,
                a.Percentual,
                a.WatchedSeconds,
                a.AccessDateTime))
            .ToListAsync(cancellationToken);

        return items.PageInMemory(query.Page, query.PageSize);
    }
}

public sealed record MyClassroomQuery(int? Page = null, int? PageSize = null) : IQuery<Paged<MyPlanItem>>;

public sealed record MyPlanItem(Guid ClassroomId, string Name, int DeliveredCount, int TotalCount);

/// <summary>Classrooms where the caller has delivery plans, with delivery progress.</summary>
internal sealed class MyClassroomHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<MyClassroomQuery, Paged<MyPlanItem>>
{
    public async ValueTask<ErrorOr<Paged<MyPlanItem>>> Handle(
        MyClassroomQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");
        }

        var plansSpec = new DeliveryPlansByStudentSpec(userId);
        var plans = await SpecificationEvaluator.Default
            .GetQuery(context.DeliveryPlans.AsNoTracking(), plansSpec)
            .ToListAsync(cancellationToken);

        var result = plans
            .GroupBy(p => p.Classroom.Id)
            .Select(g =>
            {
                var plan = g.OrderByDescending(p => p.CreatedAt).First();
                var total = plan.GetSchedule().Count;
                var delivered = plan.DeliveredClasses.Count;
                return new MyPlanItem(g.Key, plan.Classroom.Name, delivered, total);
            })
            .OrderBy(c => c.Name)
            .ToList();

        return result.PageInMemory(query.Page, query.PageSize);
    }
}
