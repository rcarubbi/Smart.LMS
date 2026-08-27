using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using ErrorOr;

namespace SmartLMS.Application.Catalog.Courses;

public sealed record ClassSummary(Guid Id, string Name, int Order, int DeliveryDays, string ContentType);

public sealed record CourseDetail(
    Guid Id,
    string Name,
    int Order,
    string? Image,
    Guid SubjectId,
    string SubjectName,
    Guid? TeacherInChargeId,
    IReadOnlyList<ClassSummary> Classes);

public sealed record CreateCourseCommand(
    string Name,
    int Order,
    Guid SubjectId,
    Guid? TeacherInChargeId = null,
    string? Image = null) : ICommand<Guid>;

internal sealed class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Order).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SubjectId).NotEmpty();
        RuleFor(c => c.Image).MaximumLength(1000);
    }
}

internal sealed class CreateCourseHandler(ISmartLmsDbContext context) : ICommandHandler<CreateCourseCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var subjectExists = await context.Subjects.AnyAsync(s => s.Id == command.SubjectId, cancellationToken);
        if (!subjectExists)
        {
            return Error.NotFound("Subjects.NotFound", "Subject not found.");
        }

        var course = new Domain.Entities.Content.Course(
            command.Name,
            command.Order,
            command.SubjectId,
            command.TeacherInChargeId,
            command.Image);

        context.Courses.Add(course);
        await context.SaveChangesAsync(cancellationToken);

        return course.Id;
    }
}

public sealed record UpdateCourseCommand(
    Guid Id,
    string Name,
    int Order,
    Guid SubjectId,
    Guid? TeacherInChargeId,
    string? Image) : ICommand;

internal sealed class UpdateCourseValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Order).GreaterThanOrEqualTo(0);
    }
}

internal sealed class UpdateCourseHandler(ISmartLmsDbContext context) : ICommandHandler<UpdateCourseCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        var course = await context.Courses.FindAsync([command.Id], cancellationToken);
        if (course is null)
        {
            return Error.NotFound("Courses.NotFound", "Course not found.");
        }

        var subjectExists = await context.Subjects.AnyAsync(s => s.Id == command.SubjectId, cancellationToken);
        if (!subjectExists)
        {
            return Error.NotFound("Subjects.NotFound", "Subject not found.");
        }

        course.Rename(command.Name);
        course.SetOrder(command.Order);
        course.MoveToSubject(command.SubjectId);
        course.AssignTeacher(command.TeacherInChargeId);
        course.ChangeImage(command.Image);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record DeleteCourseCommand(Guid Id) : ICommand;

internal sealed class DeleteCourseHandler(ISmartLmsDbContext context) : ICommandHandler<DeleteCourseCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteCourseCommand command, CancellationToken cancellationToken)
    {
        var course = await context.Courses.FindAsync([command.Id], cancellationToken);
        if (course is null)
        {
            return Error.NotFound("Courses.NotFound", "Course not found.");
        }

        var enrolledInClassrooms = await context.ClassroomCourses
            .AnyAsync(cc => cc.CourseId == command.Id, cancellationToken);

        if (enrolledInClassrooms)
        {
            return Error.Conflict(
                "Courses.EnrolledInClassrooms",
                "Cannot delete a course that is part of a classroom delivery plan.");
        }

        context.Courses.Remove(course);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record GetCourseQuery(Guid Id) : IQuery<CourseDetail>;

internal sealed class GetCourseHandler(ISmartLmsDbContext context) : IQueryHandler<GetCourseQuery, CourseDetail>
{
    public async ValueTask<ErrorOr<CourseDetail>> Handle(GetCourseQuery query, CancellationToken cancellationToken)
    {
        var detail = await context.Courses
            .Where(c => c.Id == query.Id)
            .Select(c => new CourseDetail(
                c.Id,
                c.Name,
                c.Order,
                c.Image,
                c.SubjectId,
                c.Subject.Name,
                c.TeacherInChargeId,
                c.Classes
                    .OrderBy(cl => cl.Order)
                    .Select(cl => new ClassSummary(cl.Id, cl.Name, cl.Order, cl.DeliveryDays, cl.ContentType.ToString()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return detail is null
            ? Error.NotFound("Courses.NotFound", "Course not found.")
            : detail;
    }
}

public sealed record CourseListItem(Guid Id, string Name, int Order, Guid SubjectId, string? Image, string? TeacherInChargeName, int ClassCount);

public sealed record ListCoursesQuery(Guid? SubjectId) : IQuery<IReadOnlyList<CourseListItem>>;

internal sealed class ListCoursesHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListCoursesQuery, IReadOnlyList<CourseListItem>>
{
    public async ValueTask<ErrorOr<IReadOnlyList<CourseListItem>>> Handle(
        ListCoursesQuery query,
        CancellationToken cancellationToken)
    {
        var items = await context.Courses
            .Where(c => query.SubjectId == null || c.SubjectId == query.SubjectId)
            .OrderBy(c => c.Order).ThenBy(c => c.Name)
            .Select(c => new CourseListItem(
                c.Id,
                c.Name,
                c.Order,
                c.SubjectId,
                c.Image,
                c.TeacherInCharge != null ? c.TeacherInCharge.Name : null,
                c.Classes.Count))
            .ToListAsync(cancellationToken);

        return items;
    }
}
