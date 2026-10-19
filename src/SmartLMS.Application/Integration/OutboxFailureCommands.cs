using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.Integration;
using ErrorOr;

namespace SmartLMS.Application.Integration;

public sealed record DeadLetterItem(long Id, string Type, DateTime CreatedAt, int Attempts, string? Error);
public sealed record ListDeadLettersQuery(int? Page = null, int? PageSize = null)
    : IQuery<Paged<DeadLetterItem>>;

internal sealed class ListDeadLettersHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListDeadLettersQuery, Paged<DeadLetterItem>>
{
    public async ValueTask<ErrorOr<Paged<DeadLetterItem>>> Handle(
        ListDeadLettersQuery query,
        CancellationToken cancellationToken)
    {
        var messages = context.OutboxMessages.AsNoTracking()
            .Where(m => m.ProcessedOn == null && m.Attempts >= OutboxMessage.MaxAttempts)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new DeadLetterItem(m.Id, m.Type, m.CreatedAt, m.Attempts, m.Error));
        return await messages.PageAsync(query.Page, query.PageSize, cancellationToken);
    }
}

public sealed record RetryDeadLetterCommand(long Id) : ICommand;

internal sealed class RetryDeadLetterValidator : AbstractValidator<RetryDeadLetterCommand>
{
    public RetryDeadLetterValidator() => RuleFor(c => c.Id).GreaterThan(0);
}

internal sealed class RetryDeadLetterHandler(ISmartLmsDbContext context)
    : ICommandHandler<RetryDeadLetterCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(RetryDeadLetterCommand command, CancellationToken cancellationToken)
    {
        var message = await context.OutboxMessages.SingleOrDefaultAsync(m => m.Id == command.Id, cancellationToken);
        if (message is null || message.ProcessedOn is not null)
        {
            return Error.NotFound("Outbox.NotFound", "Mensagem não encontrada.");
        }
        if (message.Attempts < OutboxMessage.MaxAttempts)
        {
            return Error.Conflict("Outbox.NotDeadLettered", "A mensagem ainda está na fila de repetição automática.");
        }

        message.Retry();
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
