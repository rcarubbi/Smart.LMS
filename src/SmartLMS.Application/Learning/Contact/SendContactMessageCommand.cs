using System.Text.Encodings.Web;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities;
using ErrorOr;

namespace SmartLMS.Application.Learning.Contact;

public sealed record ContactMessageInput(string Name, string Email, string Message);
public sealed record SendContactMessageCommand(ContactMessageInput Input) : ICommand, IOwnsTransaction;

internal sealed class SendContactMessageValidator : AbstractValidator<SendContactMessageCommand>
{
    public SendContactMessageValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Input.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Input.Message).NotEmpty().MaximumLength(5000);
    }
}

internal sealed class SendContactMessageHandler(ISmartLmsDbContext context, IEmailSender emailSender)
    : ICommandHandler<SendContactMessageCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(SendContactMessageCommand command, CancellationToken cancellationToken)
    {
        var receiver = await context.Parameters.AsNoTracking()
            .Where(p => p.Key == Parameter.TalkToUsReceiverEmailKey)
            .Select(p => p.Value)
            .FirstOrDefaultAsync(cancellationToken);

        receiver ??= await context.Users.OfType<Domain.Entities.UserAccess.Admin>()
            .Where(u => u.Active)
            .OrderBy(u => u.CreatedAt)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(receiver))
        {
            return Error.Conflict("Contact.NotConfigured", "O contato não está configurado.");
        }

        var name = HtmlEncoder.Default.Encode(command.Input.Name.Trim());
        var email = HtmlEncoder.Default.Encode(command.Input.Email.Trim());
        var message = HtmlEncoder.Default.Encode(command.Input.Message.Trim()).Replace("\n", "<br />");
        await emailSender.SendAsync(receiver, $"SmartLMS contact from {name}",
            $"<p><b>{name}</b> ({email}) sent:</p><p>{message}</p>", cancellationToken);

        return Result.Success;
    }
}
