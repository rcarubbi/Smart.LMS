using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.Communication;
using ErrorOr;

namespace SmartLMS.Application.Learning.Notices;

public sealed record SendNoticeCommand(string Text) : ICommand<Guid>;

internal sealed class SendNoticeValidator : AbstractValidator<SendNoticeCommand>
{
    public SendNoticeValidator()
    {
        RuleFor(c => c.Text).NotEmpty().MaximumLength(5000);
    }
}

internal sealed class SendNoticeHandler(
    ISmartLmsDbContext context,
    TimeProvider timeProvider) : ICommandHandler<SendNoticeCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(SendNoticeCommand command, CancellationToken cancellationToken)
    {
        var notice = new Notice(command.Text, timeProvider.GetUtcNow().UtcDateTime);

        context.Notices.Add(notice);
        await context.SaveChangesAsync(cancellationToken);

        return notice.Id;
    }
}
