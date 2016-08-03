using Carubbi.Utils.Data;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using System.Net.Mail;
using System.Web;

namespace SmartLMS.WebUI.Servicos
{
    class ServicoNotificacao
    {
        private string _baseUrl;
        private string _baseTemplatePath;
        private IContexto _contexto;
        public ServicoNotificacao(IContexto contexto)
        {
            _contexto = contexto;
            _baseTemplatePath = HttpContext.Current.Server.MapPath("~\\EmailTemplates");
            _baseUrl = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Authority}";
        }

        public string HttpContextMapPath { get; private set; }


        public void NotificarRecuperacaoSenha(string email, string senhaRecuperada, string link)
        {
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            Usuario usuario = usuarioRepo.ObterPorEmail(email);

            RepositorioParametro parametroRepo = new RepositorioParametro(_contexto);

            string corpo = $@"<!DOCTYPE html>
                            <html>
                            <head>
                                <meta name='viewport' content='width=device-width' />
                                <title>

                                </title>
                            </head>
                            <body>
                                <div>
                                    Olá {usuario.Nome}, seus dados de acesso ao Código Nerd são:
                                </div>
                                <div>
                                    Login: {email}.
                                </div>
                                <div>
                                    Senha: {senhaRecuperada}.
                                </div>
                                <div>
                                    Acesso nossa plataforma pelo endereço <a href='{_baseUrl + link}'>{_baseUrl + link}</a>.
                                </div>
                                <div>
                                    Atenciosamente, {Parametro.PROJETO}
                                </div>
                            </body>
                            </html>";


            MailMessage message = new MailMessage();
            message.To.Add(email);
            message.From = new MailAddress(parametroRepo.ObterValorPorChave(Parametro.REMETENTE_EMAIL), Parametro.PROJETO);
            message.Subject = string.Format("{0} - Recuperação de senha", Parametro.PROJETO);
            message.IsBodyHtml = true;
            message.Body = corpo;

            var sender = new Carubbi.Mailer.Implementation.SmtpSender();
            sender.Host = parametroRepo.ObterValorPorChave(Parametro.SMTP_SERVIDOR);
            sender.PortNumber = parametroRepo.ObterValorPorChave(Parametro.SMTP_PORTA).To(587);
            sender.UseSSL = parametroRepo.ObterValorPorChave(Parametro.SMTP_USA_SSL).To(true);
            sender.Username = parametroRepo.ObterValorPorChave(Parametro.SMTP_USUARIO);
            sender.Password = parametroRepo.ObterValorPorChave(Parametro.SMTP_SENHA);
            sender.UseDefaultCredentials = parametroRepo.ObterValorPorChave(Parametro.SMTP_USAR_CREDENCIAIS_PADRAO).To<bool>(false);
            sender.Send(message);
        }
    }
}
