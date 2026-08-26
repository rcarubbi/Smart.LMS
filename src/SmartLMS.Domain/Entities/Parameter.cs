namespace SmartLMS.Domain.Entities;

/// <summary>
/// Application-wide settings persisted as key/value pairs.
/// Loaded once per request scope by the application layer (no static state).
/// </summary>
public class Parameter : Entity
{
    public const string CryptoKey = "CryptoKey";
    public const string EmailFromKey = "EmailFrom";
    public const string SmtpServerKey = "SmtpServer";
    public const string SmtpPortKey = "SmtpPort";
    public const string SmtpUseSslKey = "SmtpUseSSL";
    public const string SmtpUsernameKey = "SmtpUsername";
    public const string SmtpPasswordKey = "SmtpPassword";
    public const string AppNameKey = "AppName";
    public const string FileStorageKey = "FileStorage";
    public const string TalkToUsReceiverNameKey = "TalkToUsReceiverName";
    public const string TalkToUsReceiverEmailKey = "TalkToUsReceiverEmail";
    public const string BaseUrlKey = "BaseUrl";
    public const string DaemonUserKey = "DaemonUser";

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
