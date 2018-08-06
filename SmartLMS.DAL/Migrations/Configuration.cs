using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.DAL.Migrations
{
    using Carubbi.Utils.Security;
    using System;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<SmartLMS.DAL.Context>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(SmartLMS.DAL.Context context)
        {
            var crypto = new CriptografiaSimetrica(SymmetricCryptProvider.TripleDES) { Key = "Gnatta123" };

            #region Usuario

            context.Set<User>().AddOrUpdate(u => u.Name,
                new Admin()
                {
                    Name = "Admin",
                    Active = true,
                    Login = "raphael.carubbi@gnatta.com",
                    Email = "raphael.carubbi@gnatta.com",
                    Password = crypto.Encrypt("gnatta123"),
                    CreatedAt = DateTime.Now
                });


            context.Set<User>().AddOrUpdate(u => u.Name,
                new Admin()
                {
                    Name = "Daemon User",
                    Active = true,
                    Login = "delivery.agent@gnatta.com",
                    Email = "raphael.carubbi@gnatta.com",
                    Password = crypto.Encrypt("gnatta123"),
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
                        Value = "Gnatta123",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.EMAIL_FROM_KEY,
                        Value = "noreply@gnatta.com",
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
                        Value = "user@gnatta.com",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.APP_NAME_KEY,
                        Value = "Gnatta Training Tool",
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
                        Value = "Jon Ratcliffe",
                        Active = true
                    },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.TALK_TO_US_RECEIVER_EMAIL_KEY,
                        Value = "jon.ratcliffe@gnatta.com",
                        Active = true
                    },
                      new Parameter
                      {
                          CreatedAt = DateTime.Now,
                          Key = Parameter.DELIVERED_CLASS_NOTICE_BODY_KEY,
                          Value = $"Hi {{Name}}, how are you? <br /> The class <a href='www.codigonerd.net/SmartLMS/Class/Watch/{{ClassId}}'>{{Class}}</a> is now avaialble on the course <a href='www.codigonerd.net/SmartLMS/Class/Index/{{CourseId}}'>{{Course}}</a> <br />Have a productive study! <br /><br /> {Parameter.APP_NAME}",
                          Active = true
                      },
                    new Parameter
                    {
                        CreatedAt = DateTime.Now,
                        Key = Parameter.DAEMON_USER_KEY,
                        Value = "delivery.agent@gnatta.com",
                        Active = true
                    });



            context.Save();

            #endregion
        }
    }
}




