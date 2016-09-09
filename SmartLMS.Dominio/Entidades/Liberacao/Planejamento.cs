using Carubbi.Mailer.Interfaces;
using Carubbi.Utils.Data;
using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Pessoa;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;


namespace SmartLMS.Dominio.Entidades.Liberacao
{
    public class Planejamento
    {
        public Planejamento()
        {
            Alunos = new List<Aluno>();
            AulasDisponiveis = new List<AulaPlanejamento>();
        }

        public virtual ICollection<Aviso> Avisos { get; set; }

        public long Id { get; set; }

        public virtual ICollection<AulaPlanejamento> AulasDisponiveis { get; set; }

        public virtual ICollection<Aluno> Alunos { get; set; }
        public virtual Turma Turma { get; set; }

        public DateTime DataInicio { get; set; }

        public bool Concluido { get; set; }

        public void LiberarAcessosPendentes(IContexto contexto, IMailSender sender)
        {
            if (Concluido)
                return;

            var aulaLiberada = false;
          
            do
            {

                var ultimaAulaLiberada = AulasDisponiveis.OrderByDescending(x => x.DataLiberacao).FirstOrDefault();
                var proximoCurso = ObterProximoCurso(ultimaAulaLiberada);

                var proximaAula = ObterProximaAula(ultimaAulaLiberada, ultimaAulaLiberada?.Aula?.Curso);
                if (proximaAula == null && (ultimaAulaLiberada == null || (ultimaAulaLiberada != null && ultimaAulaLiberada.DataLiberacao.Date < DateTime.Today)))
                {
                    proximaAula = ObterProximaAula(ultimaAulaLiberada, proximoCurso);
                }

                if (proximaAula == null)
                {
                    Concluido = proximoCurso == null;
                    aulaLiberada = false;
                    contexto.Salvar();
                }
                else
                {
                    aulaLiberada = VerificarLiberacao(contexto, sender, proximaAula);
                }
            } while (aulaLiberada);
        }

        internal void DisponibilizarAula(IContexto contexto, IMailSender sender, Aula aula)
        {
            AulasDisponiveis.Add(new AulaPlanejamento
            {
                Aula = aula,
                DataLiberacao = DateTime.Now,
                Planejamento = this
            });

            contexto.ObterLista<Aviso>().Add(new Aviso
            {
                Texto = $@"Novo <a href='Aula/Ver/{aula.Id}'>{Parametro.AULA} {aula.Nome}</a> disponível! <br />
                               <a href='Aula/Index/{aula.Curso.Id}'>{Parametro.CURSO} {aula.Curso.Nome}</a> <br />",
                DataHora = DateTime.Now,
                Planejamento = this,
            });
            contexto.Salvar();
            EnviarEmailsLiberacaoAula(contexto, sender, aula, Alunos);
        }

        private bool VerificarLiberacao(IContexto contexto, IMailSender sender, Aula aula)
        {
            DateTime? dataUltimaLiberacao = AulasDisponiveis.OrderByDescending(x => x.DataLiberacao).Select(x => x.DataLiberacao).FirstOrDefault();

            if ((dataUltimaLiberacao ?? DataInicio).AddDays(aula.DiasLiberacao) <= DateTime.Now)
            {
                DisponibilizarAula(contexto, sender, aula);
             
                return true;
            }
            return false;
        }

        public void EnviarEmailsLiberacaoAula(IContexto contexto, IMailSender sender, Aula aula, ICollection<Aluno> alunos)
        {
            foreach (var aluno in alunos)
            {
                EnviarEmailLiberacaoAula(contexto, sender, aula, aluno);
            }
        }

        public void EnviarEmailLiberacaoAula(IContexto contexto, IMailSender sender, Aula aula, Aluno aluno)
        {

            sender.PortNumber = contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_PORTA).Valor.To(0);
            sender.Host = contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_SERVIDOR).Valor;
            sender.UseDefaultCredentials = contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_USAR_CREDENCIAIS_PADRAO).Valor.To(false);
            sender.UseSSL = contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_USA_SSL).Valor.To(false);
            if (!sender.UseDefaultCredentials)
            {
                sender.Username = ConfigurationManager.AppSettings["SMTPUsuario"] ?? contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_USUARIO).Valor;
                sender.Password = ConfigurationManager.AppSettings["SMTPSenha"] ?? contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_SENHA).Valor;
            }

           
         
            var emailRemetente = contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.REMETENTE_EMAIL).Valor;

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
            sender.Send(email);
        }

        private Curso ObterProximoCurso(AulaPlanejamento ultimaAulaLiberada)
        {
            var ordem = 0;
            if (ultimaAulaLiberada != null)
            {
                ordem = Turma.Cursos
                    .Where(x => x.IdCurso == ultimaAulaLiberada.Aula.Curso.Id)
                    .Single().Ordem;
            }


            var curso = Turma.Cursos
                .Where(a => a.Curso.Ativo)
                .OrderBy(x => x.Ordem)
                .FirstOrDefault(x => x.Ordem > ordem);

            return curso?.Curso;
        }

        private Aula ObterProximaAula(AulaPlanejamento ultimaAulaLiberada, Curso curso)
        {
            if (curso == null)
                return null;
            var ordem = 0;
            if (ultimaAulaLiberada != null)
            {
                if (ultimaAulaLiberada.Aula.Curso.Id == curso.Id)
                {
                    ordem = ultimaAulaLiberada.Aula.Ordem;
                }
                else
                    ordem = 0;
            }

            return curso.Aulas.Where(a => a.Ativo).OrderBy(x => x.Ordem).FirstOrDefault(x => x.Ordem > ordem);
        }

    }
}
