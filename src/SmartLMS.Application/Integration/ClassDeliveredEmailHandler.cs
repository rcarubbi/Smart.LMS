using System.Text.Json;
using SmartLMS.Application.Abstractions;
using SmartLMS.Domain.Events.Delivery;

namespace SmartLMS.Application.Integration;

/// <summary>Notifies each enrolled student by e-mail when a delivery plan releases a class.</summary>
internal sealed class ClassDeliveredEmailHandler(IEmailSender emailSender) : IOutboxEventHandler
{
    public string MessageType => nameof(ClassDeliveredDomainEvent);

    public async ValueTask HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var evt = JsonSerializer.Deserialize<ClassDeliveredDomainEvent>(payload, JsonOptions)
            ?? throw new InvalidOperationException("Could not deserialize ClassDeliveredDomainEvent payload.");

        foreach (var recipient in evt.Recipients)
        {
            await emailSender.SendAsync(
                recipient.Email,
                $"New class available: {evt.ClassName}",
                $"Hello {recipient.Name},<br/>The class <b>{evt.ClassName}</b> of course <b>{evt.CourseName}</b> ({evt.SubjectName}) has just been released in classroom {evt.ClassroomName}.",
                cancellationToken);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };
}