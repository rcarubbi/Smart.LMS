using Ardalis.Specification.EntityFrameworkCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Specifications;
using ErrorOr;

namespace SmartLMS.Application.Learning.Notices;

public enum NoticeAudience
{
    All,
    Global,
    Delivery,
    Personal,
}

public sealed record NoticeItem(
    Guid Id,
    string Text,
    DateTime DateTime,
    bool Seen,
    Guid? DeliveryPlanId);

public sealed record ListMyNoticesQuery(
    int? Page = null,
    int? PageSize = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    NoticeAudience Audience = NoticeAudience.All)
    : IQuery<Paged<NoticeItem>>;

internal sealed class ListMyNoticesValidator : AbstractValidator<ListMyNoticesQuery>
{
    public ListMyNoticesValidator() =>
        RuleFor(q => q).Must(q => !q.StartDate.HasValue || !q.EndDate.HasValue || q.StartDate <= q.EndDate)
            .WithMessage("The start date must be before the end date.");
}

internal sealed class ListMyNoticesHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<ListMyNoticesQuery, Paged<NoticeItem>>
{
    public async ValueTask<ErrorOr<Paged<NoticeItem>>> Handle(
        ListMyNoticesQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");
        }

        var spec = new MyNoticesSpec(userId);
        var filtered = SpecificationEvaluator.Default.GetQuery(context.Notices.AsNoTracking(), spec);
        if (query.StartDate is { } startDate)
        {
            filtered = filtered.Where(n => n.DateTime >= startDate.Date);
        }

        if (query.EndDate is { } endDate)
        {
            filtered = filtered.Where(n => n.DateTime < endDate.Date.AddDays(1));
        }

        filtered = query.Audience switch
        {
            NoticeAudience.Global => filtered.Where(n => n.UserId == null && n.DeliveryPlanId == null),
            NoticeAudience.Delivery => filtered.Where(n => n.DeliveryPlanId != null),
            NoticeAudience.Personal => filtered.Where(n => n.UserId == userId),
            _ => filtered,
        };

        var notices = await filtered
            .Select(n => new NoticeItem(
                n.Id,
                n.Text,
                n.DateTime,
                n.Users.Any(un => un.UserId == userId),
                n.DeliveryPlanId))
            .PageAsync(query.Page, query.PageSize, cancellationToken);

        return notices;
    }
}

public sealed record MarkNoticeSeenCommand(Guid NoticeId) : ICommand;

internal sealed class MarkNoticeSeenHandler(ISmartLmsDbContext context, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<MarkNoticeSeenCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(MarkNoticeSeenCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");
        }

        var visible = await SpecificationEvaluator.Default
            .GetQuery(context.Notices.AsNoTracking(), new MyNoticesSpec(userId))
            .AnyAsync(n => n.Id == command.NoticeId, cancellationToken);
        if (!visible)
        {
            return Error.NotFound("Notices.NotFound", "Notice not found.");
        }

        var alreadySeen = await context.UserNotices
            .AnyAsync(un => un.NoticeId == command.NoticeId && un.UserId == userId, cancellationToken);

        if (!alreadySeen)
        {
            context.UserNotices.Add(new Domain.Entities.Communication.UserNotice(
                userId,
                command.NoticeId,
                timeProvider.GetUtcNow().UtcDateTime));
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success;
    }
}
