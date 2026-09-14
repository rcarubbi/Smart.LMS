using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Catalog.Classes;
using SmartLMS.Application.Integration;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.Integration;
using SmartLMS.Domain.Events.Delivery;

namespace SmartLMS.UnitTests.Application;

public class OutboxTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static OutboxMessage DeliveredMessage(Guid planId, string email = "student@smartlms.local")
    {
        var evt = new ClassDeliveredDomainEvent(
            planId,
            Guid.NewGuid(),
            "Queues 101",
            Guid.NewGuid(),
            "Message Queues",
            "Distributed Systems",
            "Night Class",
            [new ClassDeliveredRecipient(Guid.NewGuid(), "Helen", email)],
            DateTime.UtcNow);
        return new OutboxMessage(
            nameof(ClassDeliveredDomainEvent),
            JsonSerializer.Serialize(evt, JsonOptions),
            correlationId: planId);
    }

    [Test]
    public async Task ProcessOutbox_sends_email_and_marks_processed()
    {
        var planId = Guid.NewGuid();
        var message = DeliveredMessage(planId);

        var outbox = Substitute.For<IOutboxMessageRepository>();
        outbox.GetUnprocessedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([message]);

        var context = Substitute.For<ISmartLmsDbContext>();
        context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var email = Substitute.For<IEmailSender>();
        var handler = new ProcessOutboxMessagesHandler(
            context, outbox, new IOutboxEventHandler[] { new ClassDeliveredEmailHandler(email) },
            TimeProvider.System, NullLogger<ProcessOutboxMessagesHandler>.Instance);

        var result = await handler.Handle(new ProcessOutboxMessagesCommand(), CancellationToken.None);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsError).IsFalse();
            await Assert.That(result.Value).IsEqualTo(1);
            await Assert.That(message.ProcessedOn).IsNotNull();
        }

        await email.Received(1).SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ProcessOutbox_marks_unknown_type_failed_and_continues()
    {
        var unknown = new OutboxMessage("NoSuchEvent", "{}", correlationId: null);
        var planId = Guid.NewGuid();
        var valid = DeliveredMessage(planId);

        var outbox = Substitute.For<IOutboxMessageRepository>();
        outbox.GetUnprocessedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([unknown, valid]);

        var context = Substitute.For<ISmartLmsDbContext>();
        context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var email = Substitute.For<IEmailSender>();
        var handler = new ProcessOutboxMessagesHandler(
            context, outbox, new IOutboxEventHandler[] { new ClassDeliveredEmailHandler(email) },
            TimeProvider.System, NullLogger<ProcessOutboxMessagesHandler>.Instance);

        var result = await handler.Handle(new ProcessOutboxMessagesCommand(), CancellationToken.None);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsError).IsFalse();
            await Assert.That(result.Value).IsEqualTo(1);
            await Assert.That(unknown.ProcessedOn).IsNull();
            await Assert.That(unknown.Attempts).IsEqualTo(1);
            await Assert.That(valid.ProcessedOn).IsNotNull();
        }

        await email.Received(1).SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ProcessOutbox_records_failure_when_email_throws()
    {
        var message = DeliveredMessage(Guid.NewGuid());

        var outbox = Substitute.For<IOutboxMessageRepository>();
        outbox.GetUnprocessedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([message]);

        var context = Substitute.For<ISmartLmsDbContext>();
        context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var email = Substitute.For<IEmailSender>();
        email.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("smtp down")));

        var handler = new ProcessOutboxMessagesHandler(
            context, outbox, new IOutboxEventHandler[] { new ClassDeliveredEmailHandler(email) },
            TimeProvider.System, NullLogger<ProcessOutboxMessagesHandler>.Instance);

        var result = await handler.Handle(new ProcessOutboxMessagesCommand(), CancellationToken.None);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsError).IsFalse();
            await Assert.That(result.Value).IsEqualTo(0);
            await Assert.That(message.ProcessedOn).IsNull();
            await Assert.That(message.Attempts).IsEqualTo(1);
            await Assert.That(message.Error).Contains("smtp down");
        }
    }

    [Test]
    public async Task UpdateClassValidator_rejects_negative_order_and_empty_teacher()
    {
        var validator = new UpdateClassValidator();
        var input = new UpdateClassInput(
            "Name", "https://example.com/lesson", Order: -1, DeliveryDays: -2,
            ContentType.YouTube, TeacherId: Guid.Empty);

        var result = await validator.ValidateAsync(new UpdateClassCommand(Guid.NewGuid(), input));

        using (Assert.Multiple())
        {
            await Assert.That(result.IsValid).IsFalse();
            await Assert.That(result.Errors.Count).IsGreaterThanOrEqualTo(3);
        }
    }

    [Test]
    public async Task OutboxMessage_failure_semantics_match_repository_cutoff()
    {
        var message = new OutboxMessage("T", "{}", correlationId: null);

        for (var i = 0; i < 5; i++)
        {
            message.MarkFailed($"boom {i}");
        }

        using (Assert.Multiple())
        {
            // Repository filters Attempts < 5, so 5 attempts means dead-letter.
            await Assert.That(message.Attempts).IsEqualTo(5);
            await Assert.That(message.ProcessedOn).IsNull();
        }

        message.MarkProcessed(DateTime.UtcNow);
        await Assert.That(message.ProcessedOn).IsNotNull();
    }
}
