using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.UserAccess;
using ErrorOr;

namespace SmartLMS.Application.Catalog.Classes.Comments;

public sealed record CommentItem(
    Guid Id,
    string CommentText,
    string Username,
    Guid UserId,
    DateTime DateTime);

public sealed record ListClassCommentsQuery(Guid ClassId) : IQuery<IReadOnlyList<CommentItem>>;

internal sealed class ListClassCommentsHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListClassCommentsQuery, IReadOnlyList<CommentItem>>
{
    public async ValueTask<ErrorOr<IReadOnlyList<CommentItem>>> Handle(
        ListClassCommentsQuery query,
        CancellationToken cancellationToken)
    {
        var comments = await context.Comments
            .AsNoTracking()
            .Where(c => c.ClassId == query.ClassId)
            .OrderByDescending(c => c.DateTime)
            .Select(c => new CommentItem(
                c.Id,
                c.CommentText,
                c.User.Name,
                c.UserId,
                c.DateTime))
            .ToListAsync(cancellationToken);

        return comments;
    }
}

public sealed record AddClassCommentCommand(Guid ClassId, string CommentText) : ICommand<Guid>;

internal sealed class AddClassCommentValidator : AbstractValidator<AddClassCommentCommand>
{
    public AddClassCommentValidator()
    {
        RuleFor(c => c.ClassId).NotEmpty();
        RuleFor(c => c.CommentText).NotEmpty().MaximumLength(2000);
    }
}

internal sealed class AddClassCommentHandler(
    ISmartLmsDbContext context,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : ICommandHandler<AddClassCommentCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(AddClassCommentCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");
        }

        var classExists = await context.Classes.AnyAsync(c => c.Id == command.ClassId, cancellationToken);
        if (!classExists)
        {
            return Error.NotFound("Classes.NotFound", "Class not found.");
        }

        var comment = new Comment(
            command.CommentText,
            command.ClassId,
            userId,
            timeProvider.GetUtcNow().UtcDateTime);

        context.Comments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}

public sealed record DeleteClassCommentCommand(Guid ClassId, Guid CommentId) : ICommand;

internal sealed class DeleteClassCommentHandler(
    ISmartLmsDbContext context,
    ICurrentUser currentUser) : ICommandHandler<DeleteClassCommentCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteClassCommentCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");
        }

        var comment = await context.Comments.FindAsync([command.CommentId], cancellationToken);
        if (comment is null || comment.ClassId != command.ClassId)
        {
            return Error.NotFound("Comments.NotFound", "Comment not found.");
        }

        var isOwner = comment.UserId == userId;
        var isAdmin = currentUser.Role == UserRole.Admin;

        if (!isOwner && !isAdmin)
        {
            return Error.Forbidden("Comments.NotAuthorized", "You cannot delete this comment.");
        }

        context.Comments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
