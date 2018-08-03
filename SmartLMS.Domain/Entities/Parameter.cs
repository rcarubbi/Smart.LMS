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
        public const string KNOWLEDGE_AREA_PLURAL_KEY = "KnowledgeAreaPlural";
        public const string KNOWLEDGE_AREA_KEY = "KnowledgeAreaName";
        public const string SUBJECT_PLURAL_KEY = "SubjectPlural";
        public const string SUBJECT_KEY = "SubjectName";
        public const string COURSE_PLURAL_KEY = "CoursePlural";
        public const string COURSE_KEY = "CourseName";
        public const string CLASS_PLURAL_KEY = "ClassPlural";
        public const string CLASS_KEY = "ClassName";
        public const string FILE_KEY = "FileName";
        public const string APP_NAME_KEY = "AppName";
        public const string FILE_STORAGE_KEY = "FileStorage";
        public const string WATCHED_CLASSES_TITLE_KEY = "WatchedClassesTitle";
        public const string LAST_CLASSES_TITLE_KEY = "LastClassesTitle";
        public const string TALK_TO_US_RECEIVER_NAME_KEY = "TalkToUsReceiverName";
        public const string TALK_TO_US_RECEIVER_EMAIL_KEY = "TalkToUsReceiverEmail";
        public const string DELIVERED_CLASS_NOTICE_BODY_KEY = "DeliveredClassNoticeBody";


        public static string APP_NAME { get; set; }
        

        public string Key { get; set; }

        public string Value { get; set; }

        public static string KNOWLEDGE_AREA_PLURAL { get; set; }
        public static string KNOWLEDGE_AREA { get;  set; }

        public static string SUBJECT_PLURAL { get; set; }
        public static string SUBJECT { get;  set; }

        public static string COURSE_PLURAL { get; set; }
        public static string COURSE { get;  set; }

        public static string CLASS_PLURAL { get; set; }
        public static string CLASS { get;  set; }

        public static string FILE { get;  set; }

        public static string WATCHED_CLASSES_TITLE { get; set; }
        
        public static string LAST_CLASSES_TITLE { get; set; }

        public static string FILE_STORAGE { get; set; }
        public static string DELIVERED_CLASS_NOTICE_BODY { get; set; }
    }
}
