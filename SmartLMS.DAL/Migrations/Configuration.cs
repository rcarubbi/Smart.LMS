using System;
using System.Data.Entity.Migrations;
using Carubbi.Security;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.DAL.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Context>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Context context)
        {
            var crypto = new SymmetricCrypt(SymmetricCryptProvider.TripleDES) {Key = "smart123"};

            #region User

            context.Set<User>().AddOrUpdate(u => u.Name,
                new Admin
                {
                    Name = "Admin",
                    Active = true,
                    Login = "administrator@yourcompany.com",
                    Email = "administrator@yourcompany.com",
                    Password = crypto.Encrypt("smart123"),
                    CreatedAt = DateTime.Now
                });


            context.Set<User>().AddOrUpdate(u => u.Name,
                new Admin
                {
                    Name = "Daemon User",
                    Active = true,
                    Login = "delivery.agent@yourcompany.com",
                    Email = "delivery.agent@yourcompany.com",
                    Password = crypto.Encrypt("smart123"),
                    CreatedAt = DateTime.Now
                });

            context.Save();

            #endregion

            #region Parametro

            context.Set<Parameter>()
                .AddOrUpdate(p => p.Key,
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.CRYPTO_KEY,
                        Value = "smart123",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.EMAIL_FROM_KEY,
                        Value = "noreply@yourcompany.com",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.SMTP_PORT_KEY,
                        Value = "25",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.SMTP_PASSWORD_KEY,
                        Value = "",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.SMTP_SERVER_KEY,
                        Value = "localhost",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.SMTP_USE_SSL_KEY,
                        Value = "false",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.SMTP_USERNAME_KEY,
                        Value = "",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.APP_NAME_KEY,
                        Value = "Your Company Training",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.SMTP_USE_DEFAULT_CREDENTIALS_KEY,
                        Value = "true",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.FILE_STORAGE_KEY,
                        Value = "Content/Support",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.TALK_TO_US_RECEIVER_NAME_KEY,
                        Value = "Raphael Carubbi Neto",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.TALK_TO_US_RECEIVER_EMAIL_KEY,
                        Value = "talk-to-us@yourcompany.com",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.DAEMON_USER_KEY,
                        Value = "delivery.agent@yourcompany.com",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.BASE_URL_KEY,
                        Value = "http://localhost:21114",
                        Active = true
                    });


            context.Save();

            #endregion
        }
    }
}