using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.UserAccess;
using ErrorOr;

namespace SmartLMS.Application.Users.DeleteUserPermanently;

/// <summary>Permanently removes a teacher or student and all user-owned history.</summary>
public sealed record DeleteUserPermanentlyCommand(Guid Id) : ICommand;

internal sealed class DeleteUserPermanentlyHandler(ISmartLmsDbContext context)
    : ICommandHandler<DeleteUserPermanentlyCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(
        DeleteUserPermanentlyCommand command,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.ClassAccesses)
            .Include(u => u.FileAccesses)
            .Include(u => u.VisitedNotices)
            .Include(u => u.PrivateNotices)
            .SingleOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

        if (user is null)
        {
            return Error.NotFound("Users.NotFound", "User not found.");
        }

        if (user is Admin)
        {
            return Error.Conflict("Users.CannotDeleteAdmin", "Não é possível excluir um administrador.");
        }

        if (user is Teacher)
        {
            var assignedToCourse = await context.Courses.AnyAsync(c => c.TeacherInChargeId == command.Id, cancellationToken);
            var assignedToClass = await context.Classes.AnyAsync(c => c.TeacherId == command.Id, cancellationToken);
            if (assignedToCourse || assignedToClass)
            {
                return Error.Conflict(
                    "Users.TeacherAssigned",
                    "Não é possível excluir definitivamente um professor que é responsável por curso ou ministra aulas. Desvincule-o antes.");
            }
        }

        if (user is Student student)
        {
            var plans = await context.DeliveryPlans
                .Include(p => p.Students)
                .Where(p => p.Students.Any(s => s.Id == command.Id))
                .ToListAsync(cancellationToken);

            foreach (var plan in plans)
            {
                plan.Students.Remove(student);
            }
        }

        var comments = await context.Comments
            .Where(c => c.UserId == command.Id)
            .ToListAsync(cancellationToken);

        if (comments.Count > 0)
        {
            context.Comments.RemoveRange(comments);
        }

        if (user.ClassAccesses.Count > 0)
        {
            context.ClassAccesses.RemoveRange(user.ClassAccesses);
        }

        if (user.FileAccesses.Count > 0)
        {
            context.FileAccesses.RemoveRange(user.FileAccesses);
        }

        if (user.VisitedNotices.Count > 0)
        {
            context.UserNotices.RemoveRange(user.VisitedNotices);
        }

        if (user.PrivateNotices.Count > 0)
        {
            context.Notices.RemoveRange(user.PrivateNotices);
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}