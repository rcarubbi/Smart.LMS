using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.UserAccess;
using ErrorOr;

namespace SmartLMS.Application.Learning.Teaching;

public sealed record TeachingCourseClass(Guid Id, string Name, int Order, bool Active);
public sealed record TeachingCourseItem(
    Guid Id,
    string Name,
    int Order,
    Guid? TeacherInChargeId,
    IReadOnlyList<TeachingCourseClass> Classes);

public sealed record MyTeachingCoursesQuery(int? Page = null, int? PageSize = null)
    : IQuery<Paged<TeachingCourseItem>>;

internal sealed class MyTeachingCoursesHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<MyTeachingCoursesQuery, Paged<TeachingCourseItem>>
{
    public async ValueTask<ErrorOr<Paged<TeachingCourseItem>>> Handle(
        MyTeachingCoursesQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || currentUser.Role is not (UserRole.Teacher or UserRole.Admin))
        {
            return Error.Forbidden("Teaching.NotAuthorized", "Acesso restrito a professores.");
        }

        var courses = context.Courses.AsNoTracking()
            .Where(c => currentUser.Role == UserRole.Admin || c.TeacherInChargeId == userId || c.Classes.Any(k => k.TeacherId == userId))
            .OrderBy(c => c.Name)
            .Select(c => new TeachingCourseItem(
                c.Id,
                c.Name,
                c.Order,
                c.TeacherInChargeId,
                c.Classes
                    .Where(k => currentUser.Role == UserRole.Admin || c.TeacherInChargeId == userId || k.TeacherId == userId)
                    .OrderBy(k => k.Order)
                    .Select(k => new TeachingCourseClass(k.Id, k.Name, k.Order, k.Active))
                    .ToList()));

        return await courses.PageAsync(query.Page, query.PageSize, cancellationToken);
    }
}

public sealed record TeachingProgressQuery(
    Guid? CourseId = null,
    Guid? ClassId = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Paged<TeachingProgressItem>>;

public sealed record TeachingProgressItem(
    Guid StudentId,
    string StudentName,
    string StudentEmail,
    Guid ClassId,
    string ClassName,
    string CourseName,
    int Percentual,
    decimal WatchedSeconds,
    DateTime LastAccessAtUtc);

public sealed record TeachingStudentsQuery(
    Guid? CourseId = null,
    Guid? ClassId = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Paged<TeachingStudentItem>>;

public sealed record TeachingStudentItem(Guid Id, string Name, string Email, string ClassroomName);

public sealed record TeachingDeliveryHistoryQuery(Guid? CourseId = null, int? Page = null, int? PageSize = null)
    : IQuery<Paged<TeachingDeliveryHistoryItem>>;
public sealed record TeachingDeliveryHistoryItem(
    Guid ClassId,
    string ClassName,
    string CourseName,
    string ClassroomName,
    DateTime ReleasedAtUtc,
    int StudentCount);

internal sealed class TeachingDeliveryHistoryHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<TeachingDeliveryHistoryQuery, Paged<TeachingDeliveryHistoryItem>>
{
    public async ValueTask<ErrorOr<Paged<TeachingDeliveryHistoryItem>>> Handle(
        TeachingDeliveryHistoryQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || currentUser.Role is not (UserRole.Teacher or UserRole.Admin))
        {
            return Error.Forbidden("Teaching.NotAuthorized", "Acesso restrito a professores.");
        }

        var deliveries = context.ClassDeliveryPlans.AsNoTracking()
            .Where(d => currentUser.Role == UserRole.Admin || d.Class.TeacherId == userId || d.Class.Course.TeacherInChargeId == userId);
        if (query.CourseId.HasValue)
        {
            deliveries = deliveries.Where(d => d.Class.CourseId == query.CourseId.Value);
        }

        return await deliveries.OrderByDescending(d => d.DeliveryDate)
            .Select(d => new TeachingDeliveryHistoryItem(
                d.ClassId,
                d.Class.Name,
                d.Class.Course.Name,
                d.DeliveryPlan.Classroom.Name,
                d.DeliveryDate,
                d.DeliveryPlan.Students.Count(s => s.Active)))
            .PageAsync(query.Page, query.PageSize, cancellationToken);
    }
}

internal sealed class TeachingStudentsHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<TeachingStudentsQuery, Paged<TeachingStudentItem>>
{
    public async ValueTask<ErrorOr<Paged<TeachingStudentItem>>> Handle(
        TeachingStudentsQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || currentUser.Role is not (UserRole.Teacher or UserRole.Admin))
        {
            return Error.Forbidden("Teaching.NotAuthorized", "Acesso restrito a professores.");
        }

        var plans = context.DeliveryPlans.AsNoTracking()
            .Where(p => p.Classroom.Active && p.Classroom.Courses.Any(cc => cc.Course.Classes.Any(k =>
                k.Active && (currentUser.Role == UserRole.Admin || k.TeacherId == userId || k.Course.TeacherInChargeId == userId))));
        if (query.CourseId.HasValue)
        {
            plans = plans.Where(p => p.Classroom.Courses.Any(cc => cc.CourseId == query.CourseId.Value));
        }
        if (query.ClassId.HasValue)
        {
            plans = plans.Where(p => p.Classroom.Courses.Any(cc => cc.Course.Classes.Any(k => k.Id == query.ClassId.Value &&
                (currentUser.Role == UserRole.Admin || k.TeacherId == userId || k.Course.TeacherInChargeId == userId))));
        }

        var students = plans.SelectMany(p => p.Students
            .Where(s => s.Active)
            .Select(s => new { s.Id, s.Name, s.Email, ClassroomName = p.Classroom.Name }))
            .Distinct()
            .OrderBy(s => s.Name);

        var page = await students.PageAsync(query.Page, query.PageSize, cancellationToken);
        return new Paged<TeachingStudentItem>(
            page.Items.Select(s => new TeachingStudentItem(s.Id, s.Name, s.Email, s.ClassroomName)).ToList(),
            page.Total,
            page.Page,
            page.PageSize);
    }
}

internal sealed class TeachingProgressValidator : AbstractValidator<TeachingProgressQuery>
{
    public TeachingProgressValidator()
    {
        RuleFor(q => q.Page).GreaterThan(0).When(q => q.Page.HasValue);
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100).When(q => q.PageSize.HasValue);
    }
}

internal sealed class TeachingProgressHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<TeachingProgressQuery, Paged<TeachingProgressItem>>
{
    public async ValueTask<ErrorOr<Paged<TeachingProgressItem>>> Handle(
        TeachingProgressQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || currentUser.Role is not (UserRole.Teacher or UserRole.Admin))
        {
            return Error.Forbidden("Teaching.NotAuthorized", "Acesso restrito a professores.");
        }

        var accesses = context.ClassAccesses.AsNoTracking()
            .Where(a => a.Class.Active &&
                (currentUser.Role == UserRole.Admin || a.Class.TeacherId == userId || a.Class.Course.TeacherInChargeId == userId));
        if (query.CourseId.HasValue)
        {
            accesses = accesses.Where(a => a.Class.CourseId == query.CourseId.Value);
        }
        if (query.ClassId.HasValue)
        {
            accesses = accesses.Where(a => a.ClassId == query.ClassId.Value);
        }

        return await accesses.OrderByDescending(a => a.AccessDateTime)
            .Select(a => new TeachingProgressItem(
                a.UserId,
                a.User.Name,
                a.User.Email,
                a.ClassId,
                a.Class.Name,
                a.Class.Course.Name,
                a.Percentual,
                a.WatchedSeconds,
                a.AccessDateTime))
            .PageAsync(query.Page, query.PageSize, cancellationToken);
    }
}
