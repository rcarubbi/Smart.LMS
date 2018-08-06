using Carubbi.Mailer.Interfaces;
using Carubbi.Utils.Data;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;
using System.Linq;
using System.Net.Mail;
using System;

namespace SmartLMS.Domain.Services
{
    public class NotificationService
    {
        
        private readonly IContext _contexto;
        private readonly IMailSender _sender;
        public NotificationService(IContext contexto, IMailSender sender)
        {
            _sender = sender;
            _contexto = contexto;
            ConfigurarSender();
            
        }

        public string HttpContextMapPath { get; private set; }


        public void SendRecoverPasswordNotification(string email, string recoveredPassword, string link)
        {
            var userRepository = new UserRepository(_contexto);
            var user = userRepository.GetByEmail(email);

            var parametroRepo = new ParameterRepository(_contexto);

            var corpo = $@"<div>
                                    Hi {user.Name}, your {Parameter.APP_NAME} credendials are:
                                </div>
                                <div>
                                    Login: {email}.
                                </div>
                                <div>
                                    Password: {recoveredPassword}.
                                </div>
                                <div>
                                    Access the platform by the following link <a href='{link}'>{link}</a>.
                                </div>
                                <div>
                                    Regards, {Parameter.APP_NAME}
                                </div>";


            var message = new MailMessage();
            message.To.Add(email);
            message.From = new MailAddress(parametroRepo.GetValueByKey(Parameter.EMAIL_FROM_KEY), Parameter.APP_NAME);
            message.Subject = $"{Parameter.APP_NAME} - Password recovery";
            message.IsBodyHtml = true;
            message.Body = corpo;
            
            _sender.Send(message);
        }


        private void ConfigurarSender()
        {
            _sender.PortNumber = _contexto.GetList<Parameter>().Single(x => x.Key == Parameter.SMTP_PORT_KEY).Value.To(0);
            _sender.Host = _contexto.GetList<Parameter>().Single(x => x.Key == Parameter.SMTP_SERVER_KEY).Value;
            _sender.UseDefaultCredentials = _contexto.GetList<Parameter>().Single(x => x.Key == Parameter.SMTP_USE_DEFAULT_CREDENTIALS_KEY).Value.To(false);
            _sender.UseSSL = _contexto.GetList<Parameter>().Single(x => x.Key == Parameter.SMTP_USE_SSL_KEY).Value.To(false);
            if (!_sender.UseDefaultCredentials)
            {
                _sender.Username = _contexto.GetList<Parameter>().Single(x => x.Key == Parameter.SMTP_USERNAME_KEY).Value;
                _sender.Password = _contexto.GetList<Parameter>().Single(x => x.Key == Parameter.SMTP_PASSWORD_KEY).Value;
            }
        }

        public void SendTalkToUsMessage(string nome, string email, string mensagem)
        {
            var emailDestinatarioFaleConosco = _contexto.GetList<Parameter>().Single(x => x.Key == Parameter.TALK_TO_US_RECEIVER_EMAIL_KEY).Value;
            var nomeDestinatarioFaleConosco = _contexto.GetList<Parameter>().Single(x => x.Key == Parameter.TALK_TO_US_RECEIVER_NAME_KEY).Value;
            var emailRemetente = _contexto.GetList<Parameter>().Single(x => x.Key == Parameter.EMAIL_FROM_KEY).Value;

            MailMessage mailMessage = new MailMessage();
            MailAddress destinatario = new MailAddress(emailDestinatarioFaleConosco, nomeDestinatarioFaleConosco);
            mailMessage.To.Add(destinatario);
            mailMessage.From = new MailAddress(emailRemetente, Parameter.APP_NAME);
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = $@"<div><h1>Talk to us - {Parameter.APP_NAME}</h1>
                                    <dl>
                                        <dt>Name:</dt>
                                        <dd>{nome}</dd>
                                        <dt>Email:</dt>
                                        <dd>{email}</dd>
                                        <dt>Message:</dt>
                                        <dd>{mensagem}</dd>
                                    </dl>
                                    </div>";
            mailMessage.Subject = $"Talk to us - {Parameter.APP_NAME}";
            _sender.Send(mailMessage);
        }

        public void SendDeliveryClassEmail(Class klass, Student student)
        {
            var emailRemetente = _contexto.GetList<Parameter>().Single(x => x.Key == Parameter.EMAIL_FROM_KEY).Value;

            var email = new MailMessage();
            var destinatario = new MailAddress(student.Email, student.Name);
            email.To.Add(destinatario);
            email.From = new MailAddress(emailRemetente, Parameter.APP_NAME);
            email.IsBodyHtml = true;
            email.Body = Parameter.DELIVERED_CLASS_NOTICE_BODY
                .Replace("{Name}", student.Name)
                .Replace("{Class}", klass.Name)
                .Replace("{ClassId}", klass.Id.ToString())
                .Replace("{Course}", klass.Course.Name)
                .Replace("{CourseId}", klass.Course.Id.ToString());

            email.Subject = $"{Parameter.APP_NAME} - New available class";
            _sender.Send(email);
        }

        internal void SendCreatingUserNotiication(User user, string password, string link)
        {
            
            var parametroRepo = new ParameterRepository(_contexto);

            var corpo = $@"<div>
                                    Hi {user.Name}, your {Parameter.APP_NAME} credendials are:
                                </div>
                                <div>
                                    Login: {user.Login}.
                                </div>
                                <div>
                                    Password: {password}.
                                </div>
                                <div>
                                    Access the platform by the following link <a href='{link}'>{link}</a>.
                                </div>
                                <div>
                                    Regards, {Parameter.APP_NAME}
                                </div>";


            var message = new MailMessage();
            message.To.Add(user.Email);
            message.From = new MailAddress(parametroRepo.GetValueByKey(Parameter.EMAIL_FROM_KEY), Parameter.APP_NAME);
            message.Subject = $"Welcome to {Parameter.APP_NAME}";
            message.IsBodyHtml = true;
            message.Body = corpo;

            _sender.Send(message);
        }
    }
}
