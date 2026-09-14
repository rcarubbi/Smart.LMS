using Ardalis.Specification.EntityFrameworkCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Specifications;
using ErrorOr;

namespace SmartLMS.Application.Catalog.Courses;

public sealed record ClassSummary(Guid Id, string Name, int Order, int DeliveryDays, string ContentType, bool Active);

public sealed record CourseDetail(
    Guid Id,
    string Name,
    int Order,
    string? Image,
    Guid SubjectId,
    string SubjectName,
    Guid? TeacherInChargeId,
    IReadOnlyList<ClassSummary> Classes);

public sealed record CourseInput(
    string Name,
    int Order,
    Guid SubjectId,
    Guid? TeacherInChargeId = null,
    string? Image = null);

public sealed record CreateCourseCommand(CourseInput Input) : ICommand<Guid>;

internal sealed class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Input.Order).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Input.SubjectId).NotEmpty();
        RuleFor(c => c.Input.Image).MaximumLength(1000);
    }
}

internal sealed class CreateCourseHandler(ISmartLmsDbContext context) : ICommandHandler<CreateCourseCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var subjectExists = await context.Subjects.AnyAsync(s => s.Id == command.Input.SubjectId, cancellationToken);
        if (!subjectExists)
        {
            return Error.NotFound("Subjects.NotFound", "Subject not found.");
        }

        var course = new Domain.Entities.Content.Course(
            command.Input.Name,
            command.Input.Order,
            command.Input.SubjectId,
            command.Input.TeacherInChargeId,
            command.Input.Image);

        context.Courses.Add(course);
        await context.SaveChangesAsync(cancellationToken);

        return course.Id;
    }
}

public sealed record UpdateCourseCommand(Guid Id, CourseInput Input) : ICommand;

internal sealed class UpdateCourseValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Input.Order).GreaterThanOrEqualTo(0);
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

        var subjectExists = await context.Subjects.AnyAsync(s => s.Id == command.Input.SubjectId, cancellationToken);
        if (!subjectExists)
        {
            return Error.NotFound("Subjects.NotFound", "Subject not found.");
        }

        course.Rename(command.Input.Name);
        course.SetOrder(command.Input.Order);
        course.MoveToSubject(command.Input.SubjectId);
        course.AssignTeacher(command.Input.TeacherInChargeId);
        course.ChangeImage(command.Input.Image);
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
            return Error.NotFound("Courses.NotFound", "Curso não encontrado.");
        }

        course.Deactivate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record RestoreCourseCommand(Guid Id) : ICommand;

internal sealed class RestoreCourseHandler(ISmartLmsDbContext context) : ICommandHandler<RestoreCourseCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(RestoreCourseCommand command, CancellationToken cancellationToken)
    {
        var course = await context.Courses.FindAsync([command.Id], cancellationToken);
        if (course is null)
        {
            return Error.NotFound("Courses.NotFound", "Curso não encontrado.");
        }

        var subjectActive = await context.Subjects.AnyAsync(s => s.Id == course.SubjectId && s.Active, cancellationToken);
        if (!subjectActive)
        {
            return Error.Conflict(
                "Courses.SubjectInactive",
                "O assunto deste curso está na lixeira. Restaure o assunto antes de restaurar o curso.");
        }

        course.Activate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record DeleteCoursePermanentlyCommand(Guid Id) : ICommand;

internal sealed class DeleteCoursePermanentlyHandler(ISmartLmsDbContext context)
    : ICommandHandler<DeleteCoursePermanentlyCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteCoursePermanentlyCommand command, CancellationToken cancellationToken)
    {
        var course = await context.Courses.FindAsync([command.Id], cancellationToken);
        if (course is null)
        {
            return Error.NotFound("Courses.NotFound", "Curso não encontrado.");
        }

        var classIds = await context.Classes
            .Where(c => c.CourseId == command.Id)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (classIds.Count > 0)
        {
            var fileIds = await context.Files
                .Where(f => classIds.Contains(f.ClassId))
                .Select(f => f.Id)
                .ToListAsync(cancellationToken);

            if (fileIds.Count > 0)
            {
                context.FileAccesses.RemoveRange(
                    await context.FileAccesses.Where(a => fileIds.Contains(a.FileId)).ToListAsync(cancellationToken));
            }

            context.ClassAccesses.RemoveRange(
                await context.ClassAccesses.Where(a => classIds.Contains(a.ClassId)).ToListAsync(cancellationToken));
            context.ClassDeliveryPlans.RemoveRange(
                await context.ClassDeliveryPlans.Where(d => classIds.Contains(d.ClassId)).ToListAsync(cancellationToken));
            context.Comments.RemoveRange(
                await context.Comments.Where(c => classIds.Contains(c.ClassId)).ToListAsync(cancellationToken));
            context.Files.RemoveRange(
                await context.Files.Where(f => classIds.Contains(f.ClassId)).ToListAsync(cancellationToken));
            context.Classes.RemoveRange(
                await context.Classes.Where(c => classIds.Contains(c.Id)).ToListAsync(cancellationToken));
        }

        context.Assessments.RemoveRange(
            await context.Assessments.Where(a => a.CourseId == command.Id).ToListAsync(cancellationToken));
        context.ClassroomCourses.RemoveRange(
            await context.ClassroomCourses.Where(cc => cc.CourseId == command.Id).ToListAsync(cancellationToken));

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
        var spec = new CourseByIdSpec(query.Id);
        var detail = await SpecificationEvaluator.Default
            .GetQuery(context.Courses.AsNoTracking(), spec)
            .Select(c => new CourseDetail(
                c.Id,
                c.Name,
                c.Order,
                c.Image,
                c.SubjectId,
                c.Subject.Name,
                c.TeacherInChargeId,
                c.Classes
                    .Where(cl => cl.Active)
                    .OrderBy(cl => cl.Order)
                    .Select(cl => new ClassSummary(cl.Id, cl.Name, cl.Order, cl.DeliveryDays, cl.ContentType.ToString(), true))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return detail is null
            ? Error.NotFound("Courses.NotFound", "Course not found.")
            : detail;
    }
}

public sealed record CourseListItem(
    Guid Id,
    string Name,
    int Order,
    Guid SubjectId,
    string? Image,
    string? TeacherInChargeName,
    int ClassCount,
    bool Active);

public sealed record ListCoursesQuery(
    Guid? SubjectId,
    bool? Active = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Paged<CourseListItem>>;

internal sealed class ListCoursesHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListCoursesQuery, Paged<CourseListItem>>
{
    public async ValueTask<ErrorOr<Paged<CourseListItem>>> Handle(
        ListCoursesQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new CoursesListSpec(query.SubjectId, query.Active);
        var filtered = SpecificationEvaluator.Default.GetQuery(context.Courses.AsNoTracking(), spec);

        var items = await filtered
            .Select(c => new CourseListItem(
                c.Id,
                c.Name,
                c.Order,
                c.SubjectId,
                c.Image,
                c.TeacherInCharge != null ? c.TeacherInCharge.Name : null,
                c.Classes.Count,
                c.Active))
            .PageAsync(query.Page, query.PageSize, cancellationToken);

        return items;
    }
}
