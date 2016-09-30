using Carubbi.Mailer.Interfaces;
using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Pessoa;
using SmartLMS.Dominio.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;


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
                Texto = $@"Nova <a href='Aula/Ver/{aula.Id}'>{Parametro.AULA} {aula.Nome}</a> disponível! <br />
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

            if ((dataUltimaLiberacao ?? DataInicio).AddDays(aula.DiasLiberacao).Date <= DateTime.Today)
            {
                DisponibilizarAula(contexto, sender, aula);
             
                return true;
            }
            return false;
        }

        public void EnviarEmailsLiberacaoAula(IContexto contexto, IMailSender sender, Aula aula, ICollection<Aluno> alunos)
        {
            ServicoNotificacao servico = new ServicoNotificacao(contexto, sender);
            foreach (var aluno in alunos)
            {
                servico.EnviarEmailLiberacaoAula(aula, aluno);
                
            }
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
