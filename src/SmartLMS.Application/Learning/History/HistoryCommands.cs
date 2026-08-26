using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Common;

namespace SmartLMS.Application.Learning.History;

public sealed record RecordClassAccessCommand(Guid ClassId, int Percentual, decimal WatchedSeconds) : ICommand;

internal sealed class RecordClassAccessValidator : AbstractValidator<RecordClassAccessCommand>
{
    public RecordClassAccessValidator()
    {
        RuleFor(c => c.ClassId).NotEmpty();
        RuleFor(c => c.Percentual).InclusiveBetween(0, 100);
        RuleFor(c => c.WatchedSeconds).GreaterThanOrEqualTo(0);
    }
}

internal sealed class RecordClassAccessHandler(ISmartLmsDbContext context, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<RecordClassAccessCommand>
{
    public async ValueTask<Result> Handle(RecordClassAccessCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user."));
        }

        var classExists = await context.Classes.AnyAsync(c => c.Id == command.ClassId, cancellationToken);
        if (!classExists)
        {
            return Result.Failure(Error.NotFound("Classes.NotFound", "Class not found."));
        }

        context.ClassAccesses.Add(new Domain.Entities.History.ClassAccess
        {
            UserId = userId,
            ClassId = command.ClassId,
            Percentual = command.Percentual,
            WatchedSeconds = command.WatchedSeconds,
            AccessDateTime = timeProvider.GetUtcNow().UtcDateTime,
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record RecordFileAccessCommand(Guid FileId) : ICommand;

internal sealed class RecordFileAccessHandler(ISmartLmsDbContext context, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<RecordFileAccessCommand>
{
    public async ValueTask<Result> Handle(RecordFileAccessCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user."));
        }

        var fileExists = await context.Files.AnyAsync(f => f.Id == command.FileId, cancellationToken);
        if (!fileExists)
        {
            return Result.Failure(Error.NotFound("Files.NotFound", "File not found."));
        }

        context.FileAccesses.Add(new Domain.Entities.History.FileAccess
        {
            UserId = userId,
            FileId = command.FileId,
            AccessDateTime = timeProvider.GetUtcNow().UtcDateTime,
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record MyProgressItem(
    Guid ClassId,
    string ClassName,
    string CourseName,
    int Percentual,
    decimal WatchedSeconds,
    DateTime LastAccessAtUtc);

public sealed record MyProgressQuery : IQuery<IReadOnlyList<MyProgressItem>>;

internal sealed class MyProgressHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<MyProgressQuery, IReadOnlyList<MyProgressItem>>
{
    public async ValueTask<Result<IReadOnlyList<MyProgressItem>>> Handle(
        MyProgressQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<IReadOnlyList<MyProgressItem>>(Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user."));
        }

        var items = await context.ClassAccesses
            .Where(a => a.UserId == userId)
            .GroupBy(a => a.ClassId)
            .Select(g => new
            {
                ClassId = g.Key,
                Percentual = g.Max(a => a.Percentual),
                WatchedSeconds = g.Max(a => a.WatchedSeconds),
                LastAccess = g.Max(a => a.AccessDateTime),
                ClassName = g.Select(a => a.Class.Name).First(),
                CourseName = g.Select(a => a.Class.Course.Name).First(),
            })
            .OrderByDescending(x => x.LastAccess)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<MyProgressItem>>(
        [
            .. items.Select(x => new MyProgressItem(
                x.ClassId,
                x.ClassName,
                x.CourseName,
                x.Percentual,
                x.WatchedSeconds,
                x.LastAccess)),
        ]);
    }
}

public sealed record MyClassroomQuery : IQuery<IReadOnlyList<MyPlanItem>>;

public sealed record MyPlanItem(Guid ClassroomId, string Name, int DeliveredCount, int TotalCount);

/// <summary>Classrooms where the caller has delivery plans, with delivery progress.</summary>
internal sealed class MyClassroomHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<MyClassroomQuery, IReadOnlyList<MyPlanItem>>
{
    public async ValueTask<Result<IReadOnlyList<MyPlanItem>>> Handle(
        MyClassroomQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<IReadOnlyList<MyPlanItem>>(Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user."));
        }

        var plans = await context.DeliveryPlans
            .Include(p => p.DeliveredClasses)
            .Include(p => p.Classroom)
            .ThenInclude(c => c.Courses)
            .ThenInclude(cc => cc.Course)
            .ThenInclude(c => c.Classes)
            .Where(p => p.Students.Any(s => s.Id == userId))
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

        return Result.Success<IReadOnlyList<MyPlanItem>>(result);
    }
}
