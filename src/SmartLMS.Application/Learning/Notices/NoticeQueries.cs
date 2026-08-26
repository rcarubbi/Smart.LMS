using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Common;

namespace SmartLMS.Application.Learning.Notices;

public sealed record NoticeItem(
    Guid Id,
    string Text,
    DateTime DateTime,
    bool Seen,
    Guid? DeliveryPlanId);

public sealed record ListMyNoticesQuery : IQuery<IReadOnlyList<NoticeItem>>;

internal sealed class ListMyNoticesHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<ListMyNoticesQuery, IReadOnlyList<NoticeItem>>
{
    public async ValueTask<Result<IReadOnlyList<NoticeItem>>> Handle(
        ListMyNoticesQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<IReadOnlyList<NoticeItem>>(Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user."));
        }

        var notices = await context.Notices
            .Where(n => n.Active && (n.UserId == userId || n.UserId == null))
            .OrderByDescending(n => n.DateTime)
            .Select(n => new NoticeItem(
                n.Id,
                n.Text,
                n.DateTime,
                n.Users.Any(un => un.UserId == userId),
                n.DeliveryPlanId))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<NoticeItem>>(notices);
    }
}

public sealed record MarkNoticeSeenCommand(Guid NoticeId) : ICommand;

internal sealed class MarkNoticeSeenHandler(ISmartLmsDbContext context, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<MarkNoticeSeenCommand>
{
    public async ValueTask<Result> Handle(MarkNoticeSeenCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user."));
        }

        var noticeExists = await context.Notices.AnyAsync(n => n.Id == command.NoticeId, cancellationToken);
        if (!noticeExists)
        {
            return Result.Failure(Error.NotFound("Notices.NotFound", "Notice not found."));
        }

        var alreadySeen = await context.UserNotices
            .AnyAsync(un => un.NoticeId == command.NoticeId && un.UserId == userId, cancellationToken);

        if (!alreadySeen)
        {
            context.UserNotices.Add(new Domain.Entities.Communication.UserNotice
            {
                UserId = userId,
                NoticeId = command.NoticeId,
                VisualizationDateTime = timeProvider.GetUtcNow().UtcDateTime,
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
