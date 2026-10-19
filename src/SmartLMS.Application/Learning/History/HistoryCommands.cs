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

        var accessTime = timeProvider.GetUtcNow().UtcDateTime;
        var rows = await UpdateProgressAtomicallyAsync(context.ClassAccesses
            .Where(a => a.UserId == userId && a.ClassId == command.Input.ClassId),
            command.Input.Percentual, command.Input.WatchedSeconds, accessTime, cancellationToken);
        if (rows > 0)
        {
            return Result.Success;
        }

        var added = new Domain.Entities.History.ClassAccess(
            userId,
            command.Input.ClassId,
            command.Input.Percentual,
            command.Input.WatchedSeconds,
            accessTime);
        context.ClassAccesses.Add(added);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Two heartbeats raced on the unique (UserId, ClassId): detach our
            // loser insert, then atomically apply monotonic max values to the winner.
            if (context is DbContext db)
            {
                db.Entry(added).State = EntityState.Detached;
            }

            rows = await UpdateProgressAtomicallyAsync(context.ClassAccesses
                .Where(a => a.UserId == userId && a.ClassId == command.Input.ClassId),
                command.Input.Percentual, command.Input.WatchedSeconds, accessTime, cancellationToken);
            if (rows == 0)
            {
                throw;
            }
        }

        return Result.Success;
    }

    private static Task<int> UpdateProgressAtomicallyAsync(
        IQueryable<Domain.Entities.History.ClassAccess> source,
        int percentual,
        decimal watchedSeconds,
        DateTime accessTime,
        CancellationToken cancellationToken) =>
        source.ExecuteUpdateAsync(update => update
            .SetProperty(a => a.Percentual, a => a.Percentual < percentual ? percentual : a.Percentual)
            .SetProperty(a => a.WatchedSeconds, a => a.WatchedSeconds < watchedSeconds ? watchedSeconds : a.WatchedSeconds)
            .SetProperty(a => a.AccessDateTime, a => a.AccessDateTime < accessTime ? accessTime : a.AccessDateTime),
            cancellationToken);
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

public sealed record MyCourseQuery(int? Page = null, int? PageSize = null) : IQuery<Paged<MyCourseItem>>;
public sealed record MyCourseItem(Guid CourseId, string CourseName, int DeliveredCount, int TotalCount, int Percentual);

internal sealed class MyCoursesHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<MyCourseQuery, Paged<MyCourseItem>>
{
    public async ValueTask<ErrorOr<Paged<MyCourseItem>>> Handle(MyCourseQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");
        }

        var plans = await SpecificationEvaluator.Default
            .GetQuery(context.DeliveryPlans.AsNoTracking(), new DeliveryPlansByStudentSpec(userId))
            .ToListAsync(cancellationToken);
        var latest = new Dictionary<Guid, (DateTime CreatedAt, MyCourseItem Item)>();
        foreach (var plan in plans)
        {
            var delivered = plan.DeliveredClasses.Select(d => d.ClassId).ToHashSet();
            foreach (var course in plan.Classroom.Courses.Select(cc => cc.Course))
            {
                var classes = course.Classes.Where(c => c.Active).ToList();
                var count = classes.Count(c => delivered.Contains(c.Id));
                var item = new MyCourseItem(course.Id, course.Name, count, classes.Count,
                    classes.Count == 0 ? 0 : count * 100 / classes.Count);
                if (!latest.TryGetValue(course.Id, out var previous) || plan.CreatedAt > previous.CreatedAt)
                {
                    latest[course.Id] = (plan.CreatedAt, item);
                }
            }
        }

        var items = latest.Values.Select(x => x.Item).OrderBy(x => x.CourseName).ToList();
        return items.PageInMemory(query.Page, query.PageSize);
    }
}

public sealed record MyReleasedClassesQuery(int? Page = null, int? PageSize = null)
    : IQuery<Paged<MyReleasedClassItem>>;
public sealed record MyReleasedClassItem(Guid ClassId, string ClassName, Guid CourseId, string CourseName, DateTime ReleasedAtUtc);

internal sealed class MyReleasedClassesHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<MyReleasedClassesQuery, Paged<MyReleasedClassItem>>
{
    public async ValueTask<ErrorOr<Paged<MyReleasedClassItem>>> Handle(
        MyReleasedClassesQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");
        }

        var plans = await SpecificationEvaluator.Default
            .GetQuery(context.DeliveryPlans.AsNoTracking(), new DeliveryPlansWithGraphSpec())
            .Where(p => p.Classroom.Active && p.Students.Any(s => s.Id == userId))
            .ToListAsync(cancellationToken);
        var latest = new Dictionary<Guid, MyReleasedClassItem>();
        foreach (var plan in plans)
        {
            foreach (var delivery in plan.DeliveredClasses)
            {
                var klass = plan.Classroom.Courses.SelectMany(cc => cc.Course.Classes)
                    .FirstOrDefault(c => c.Id == delivery.ClassId && c.Active);
                if (klass is null) continue;

                var item = new MyReleasedClassItem(klass.Id, klass.Name, klass.CourseId, klass.Course.Name, delivery.DeliveryDate);
                if (!latest.TryGetValue(klass.Id, out var previous) || item.ReleasedAtUtc > previous.ReleasedAtUtc)
                {
                    latest[klass.Id] = item;
                }
            }
        }

        var items = latest.Values.OrderByDescending(x => x.ReleasedAtUtc).ToList();
        return items.PageInMemory(query.Page, query.PageSize);
    }
}
