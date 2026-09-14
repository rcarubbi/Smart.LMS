using Microsoft.Extensions.Options;
using MimeKit;
using Polly.Registry;
using SmartLMS.Application.Abstractions;

namespace SmartLMS.Infrastructure.Messaging;

/// <summary>Keys of the resilience pipelines registered in DI.</summary>
public static class ResiliencePipelines
{
    public const string Email = "email";
}

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string FromAddress { get; set; } = "no-reply@smartlms.local";
    public string FromName { get; set; } = "SmartLMS";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseSsl { get; set; }
}

internal sealed class MailKitEmailSender(
    IOptions<SmtpOptions> options,
    ResiliencePipelineProvider<string> pipelines) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;
    private readonly Polly.ResiliencePipeline _pipeline = pipelines.GetPipeline(ResiliencePipelines.Email);

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        // At-least-once: transient SMTP faults retry inside the pipeline;
        // the outbox marks processed only after this returns, so a failure
        // here becomes another attempt, never a lost e-mail.
        await _pipeline.ExecuteAsync(async ct =>
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(_options.Host, _options.Port, _options.UseSsl, ct);

            if (!string.IsNullOrEmpty(_options.Username))
            {
                await client.AuthenticateAsync(_options.Username, _options.Password ?? string.Empty, ct);
            }

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(quit: true, ct);
        }, cancellationToken);
    }
}
