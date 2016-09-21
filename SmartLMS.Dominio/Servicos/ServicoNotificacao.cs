using Carubbi.Mailer.Interfaces;
using Carubbi.Utils.Data;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Pessoa;
using SmartLMS.Dominio.Repositorios;
using System.Linq;
using System.Net.Mail;
using System;

namespace SmartLMS.Dominio.Servicos
{
    public class ServicoNotificacao
    {
        
        private IContexto _contexto;
        private IMailSender _sender;
        public ServicoNotificacao(IContexto contexto, IMailSender sender)
        {
            _sender = sender;
            _contexto = contexto;
            ConfigurarSender();
            
        }

        public string HttpContextMapPath { get; private set; }


        public void NotificarRecuperacaoSenha(string email, string senhaRecuperada, string link)
        {
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            Usuario usuario = usuarioRepo.ObterPorEmail(email);

            RepositorioParametro parametroRepo = new RepositorioParametro(_contexto);

            string corpo = $@"<div>
                                    Olá {usuario.Nome}, seus dados de acesso ao Código Nerd são:
                                </div>
                                <div>
                                    Login: {email}.
                                </div>
                                <div>
                                    Senha: {senhaRecuperada}.
                                </div>
                                <div>
                                    Acesse nossa plataforma pelo endereço <a href='{link}'>{link}</a>.
                                </div>
                                <div>
                                    Atenciosamente, {Parametro.PROJETO}
                                </div>";


            MailMessage message = new MailMessage();
            message.To.Add(email);
            message.From = new MailAddress(parametroRepo.ObterValorPorChave(Parametro.REMETENTE_EMAIL), Parametro.PROJETO);
            message.Subject = string.Format("{0} - Recuperação de senha", Parametro.PROJETO);
            message.IsBodyHtml = true;
            message.Body = corpo;
            
            _sender.Send(message);
        }


        private void ConfigurarSender()
        {
            _sender.PortNumber = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_PORTA).Valor.To(0);
            _sender.Host = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_SERVIDOR).Valor;
            _sender.UseDefaultCredentials = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_USAR_CREDENCIAIS_PADRAO).Valor.To(false);
            _sender.UseSSL = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_USA_SSL).Valor.To(false);
            if (!_sender.UseDefaultCredentials)
            {
                _sender.Username = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_USUARIO).Valor;
                _sender.Password = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_SENHA).Valor;
            }
        }

        public void EnviarFaleConosco(string nome, string email, string mensagem)
        {
            var emailDestinatarioFaleConosco = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.CHAVE_EMAIL_DESTINATARIO_FALE_CONOSCO).Valor;
            var nomeDestinatarioFaleConosco = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.CHAVE_NOME_DESTINATARIO_FALE_CONOSCO).Valor;
            var emailRemetente = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.REMETENTE_EMAIL).Valor;

            MailMessage mailMessage = new MailMessage();
            MailAddress destinatario = new MailAddress(emailDestinatarioFaleConosco, nomeDestinatarioFaleConosco);
            mailMessage.To.Add(destinatario);
            mailMessage.From = new MailAddress(emailRemetente, Parametro.PROJETO);
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = $@"<div><h1>Fale Conosco - {Parametro.PROJETO}</h1>
                                    <dl>
                                        <dt>Nome:</dt>
                                        <dd>{nome}</dd>
                                        <dt>Email:</dt>
                                        <dd>{email}</dd>
                                        <dt>Mensagem:</dt>
                                        <dd>{mensagem}</dd>
                                    </dl>
                                    </div>";
            mailMessage.Subject = $"Fale Conosco - {Parametro.PROJETO}";
            _sender.Send(mailMessage);
        }

        public void EnviarEmailLiberacaoAula(Aula aula, Aluno aluno)
        {
            var emailRemetente = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.REMETENTE_EMAIL).Valor;

            MailMessage email = new MailMessage();
            MailAddress destinatario = new MailAddress(aluno.Email, aluno.Nome);
            email.To.Add(destinatario);
            email.From = new MailAddress(emailRemetente, Parametro.PROJETO);
            email.IsBodyHtml = true;
            email.Body = Parametro.CORPO_NOTIFICACAO_AULA_LIBERADA
                .Replace("{Nome}", aluno.Nome)
                .Replace("{Aula}", aula.Nome)
                .Replace("{IdAula}", aula.Id.ToString())
                .Replace("{Curso}", aula.Curso.Nome)
                .Replace("{IdCurso}", aula.Curso.Id.ToString());

            email.Subject = $"{Parametro.PROJETO} - Nova aula disponível";
            _sender.Send(email);
        }

        internal void NotificarCriacaoUsuario(Usuario usuario, string senha, string link)
        {
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            RepositorioParametro parametroRepo = new RepositorioParametro(_contexto);

            string corpo = $@"<div>
                                    Olá {usuario.Nome}, seus dados de acesso ao Código Nerd são:
                                </div>
                                <div>
                                    Login: {usuario.Login}.
                                </div>
                                <div>
                                    Senha: {senha}.
                                </div>
                                <div>
                                    Acesse nossa plataforma pelo endereço <a href='{link}'>{link}</a>.
                                </div>
                                <div>
                                    Atenciosamente, {Parametro.PROJETO}
                                </div>";


            MailMessage message = new MailMessage();
            message.To.Add(usuario.Email);
            message.From = new MailAddress(parametroRepo.ObterValorPorChave(Parametro.REMETENTE_EMAIL), Parametro.PROJETO);
            message.Subject = string.Format("Bem vindo ao {0}", Parametro.PROJETO);
            message.IsBodyHtml = true;
            message.Body = corpo;

            _sender.Send(message);
        }
    }
}
