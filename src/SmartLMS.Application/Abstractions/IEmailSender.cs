namespace SmartLMS.Application.Abstractions;

/// <summary>Port for transactional e-mail delivery (MailKit implementation).</summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
