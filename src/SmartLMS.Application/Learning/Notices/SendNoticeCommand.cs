using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.Communication;
using ErrorOr;

namespace SmartLMS.Application.Learning.Notices;

public sealed record SendNoticeInput(string Text);

public sealed record SendNoticeCommand(SendNoticeInput Input) : ICommand<Guid>;

internal sealed class SendNoticeValidator : AbstractValidator<SendNoticeCommand>
{
    public SendNoticeValidator()
    {
        RuleFor(c => c.Input.Text).NotEmpty().MaximumLength(5000);
    }
}

internal sealed class SendNoticeHandler(
    ISmartLmsDbContext context,
    TimeProvider timeProvider) : ICommandHandler<SendNoticeCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(SendNoticeCommand command, CancellationToken cancellationToken)
    {
        var notice = new Notice(command.Input.Text, timeProvider.GetUtcNow().UtcDateTime);

        context.Notices.Add(notice);
        await context.SaveChangesAsync(cancellationToken);

        return notice.Id;
    }
}
