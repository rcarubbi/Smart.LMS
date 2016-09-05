using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Pessoa;
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

        public void LiberarAcessosPendentes(IContexto contexto)
        {
            var aulaLiberada = false;
            do
            {

                var ultimaAulaLiberada = AulasDisponiveis.OrderByDescending(x => x.DataLiberacao).FirstOrDefault();
            

                var proximaAula = ObterProximaAula(ultimaAulaLiberada, ultimaAulaLiberada?.Aula?.Curso);
                if (proximaAula == null)
                {
                    proximaAula = ObterProximaAula(ultimaAulaLiberada, ObterProximoCurso(ultimaAulaLiberada));
                }

                if (proximaAula == null)
                {
                    Concluido = true;
                    aulaLiberada = false;
                    contexto.Salvar();
                }
                else
                {
                    aulaLiberada = VerificarLiberacao(contexto, proximaAula);
                }
            } while (aulaLiberada);
        }

        private bool VerificarLiberacao(IContexto contexto, Aula aula)
        {
            if (DataInicio.AddDays(aula.DiasLiberacao) <= DateTime.Now)
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
                return true;
            }
            return false;
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
