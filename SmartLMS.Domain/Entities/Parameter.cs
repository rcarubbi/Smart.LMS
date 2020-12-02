﻿using SmartLMS.Domain.Repositories;

namespace SmartLMS.Domain.Entities
{
    public class Parameter : Entity
    {
        public const string CRYPTO_KEY = "CryptoKey";
        public const string EMAIL_FROM_KEY = "EmailFrom";
        public const string SMTP_SERVER_KEY = "SmtpServer";
        public const string SMTP_PORT_KEY = "SmtpPort";
        public const string SMTP_USE_SSL_KEY = "SmtpUseSSL";
        public const string SMTP_USERNAME_KEY = "SmtpUsername";
        public const string SMTP_PASSWORD_KEY = "SmtpPassword";
        public const string SMTP_USE_DEFAULT_CREDENTIALS_KEY = "SmtpUseDefaultCredentials";
        public const string APP_NAME_KEY = "AppName";
        public const string FILE_STORAGE_KEY = "FileStorage";
        public const string TALK_TO_US_RECEIVER_NAME_KEY = "TalkToUsReceiverName";
        public const string TALK_TO_US_RECEIVER_EMAIL_KEY = "TalkToUsReceiverEmail";
        public const string BASE_URL_KEY = "BaseUrlKey";
        public const string DAEMON_USER_KEY = "DaemonUser";

        public static string APP_NAME { get; set; }

        public static string BASE_URL { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }


        public static string FILE_STORAGE { get; set; }


        public static string DAEMON_USER { get; set; }

        public static void LoadParameters(IContext context)
        {
            var parameterRepository = new ParameterRepository(context);
            APP_NAME = parameterRepository.GetValueByKey(APP_NAME_KEY);
            FILE_STORAGE = parameterRepository.GetValueByKey(FILE_STORAGE_KEY);
            DAEMON_USER = parameterRepository.GetValueByKey(DAEMON_USER_KEY);
#if DEBUG
            BASE_URL = "http://localhost:21114/";

#else
            BASE_URL = parameterRepository.GetValueByKey(BASE_URL_KEY);
#endif
        }
    }
}