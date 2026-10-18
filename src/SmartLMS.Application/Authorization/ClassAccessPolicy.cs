using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Application.Authorization;

/// <summary>Central server-side entitlement rules for lessons, files, and course assessments.</summary>
public sealed class ClassAccessPolicy(ISmartLmsDbContext context, ICurrentUser currentUser)
{
    public IQueryable<Class> AccessibleClasses()
    {
        var classes = context.Classes.AsNoTracking();
        if (currentUser.UserId is not { } userId)
        {
            return classes.Where(_ => false);
        }

        return currentUser.Role switch
        {
            UserRole.Admin => classes,
            UserRole.Teacher => classes.Where(c => c.TeacherId == userId || c.Course.TeacherInChargeId == userId),
            UserRole.Student => classes.Where(c => c.Active && context.DeliveryPlans.Any(p =>
                p.Classroom.Active && p.Students.Any(s => s.Id == userId) &&
                p.DeliveredClasses.Any(d => d.ClassId == c.Id))),
            _ => classes.Where(_ => false),
        };
    }

    public Task<bool> CanAccessClassAsync(Guid classId, CancellationToken cancellationToken) =>
        AccessibleClasses().AnyAsync(c => c.Id == classId, cancellationToken);

    public Task<bool> CanAccessFileAsync(Guid fileId, CancellationToken cancellationToken) =>
        context.Files.AsNoTracking().AnyAsync(f => f.Id == fileId && AccessibleClasses().Any(c => c.Id == f.ClassId), cancellationToken);

    public Task<bool> CanAccessCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Task.FromResult(false);
        }

        return currentUser.Role switch
        {
            UserRole.Admin => context.Courses.AnyAsync(c => c.Id == courseId, cancellationToken),
            UserRole.Teacher => context.Courses.AnyAsync(c => c.Id == courseId &&
                (c.TeacherInChargeId == userId || c.Classes.Any(k => k.TeacherId == userId)), cancellationToken),
            UserRole.Student => context.DeliveryPlans.AnyAsync(p =>
                p.Classroom.Active &&
                p.Students.Any(s => s.Id == userId) &&
                p.Classroom.Courses.Any(cc => cc.CourseId == courseId) &&
                p.DeliveredClasses.Any(d => d.Class.CourseId == courseId), cancellationToken),
            _ => Task.FromResult(false),
        };
    }
}
