using Quartz;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Linq;

namespace AgenteLiberacaoConteudo
{
    internal class LiberacaoAcessoJob : IJob
    {

        private IContexto _contexto;
        public LiberacaoAcessoJob(IContexto contexto)
        {
            _contexto = contexto;
        }

        public void Execute(IJobExecutionContext context)
        {
            RepositorioTurma repo = new RepositorioTurma(_contexto);
            var planejamentos = repo.ListarPlanejamentosNaoConcluidos();
            foreach (var item in planejamentos)
            {
                var aulaLiberada = false;
                do
                {
                   
                    var ultimaAulaLiberada = item.AulasDisponiveis.OrderByDescending(x => x.DataLiberacao).FirstOrDefault();
                    var dataUltimaLiberacao = ultimaAulaLiberada == null ? item.DataInicio : ultimaAulaLiberada.DataLiberacao;

                    var proximaAula = ObterProximaAula(ultimaAulaLiberada, ultimaAulaLiberada?.Aula?.Curso);
                    if (proximaAula == null)
                    {
                        proximaAula = ObterProximaAula(ultimaAulaLiberada, ObterProximoCurso(ultimaAulaLiberada, item));
                    }

                    if (proximaAula == null)
                    {
                        item.Concluido = true;
                        aulaLiberada = false;
                        _contexto.Salvar();
                    }
                    else
                    {
                        aulaLiberada = VerificarLiberacao(item, proximaAula, dataUltimaLiberacao);
                    }
                } while (aulaLiberada);
            }
        }

        private bool VerificarLiberacao(Planejamento item, Aula aula, DateTime dataUltimaLiberacao)
        {
            if (dataUltimaLiberacao.AddDays(aula.DiasLiberacao) <= DateTime.Now)
            {
                item.AulasDisponiveis.Add(new AulaPlanejamento
                {
                    Aula = aula,
                    DataLiberacao = DateTime.Now,
                    Planejamento = item
                });
                _contexto.Salvar();
                return true;
            }
            return false;
        }

        private Curso ObterProximoCurso(AulaPlanejamento ultimaAulaLiberada, Planejamento item)
        {
            var ordem = 0;
            if (ultimaAulaLiberada != null)
            {
                ordem = item.Turma.Cursos
                    .Where(x => x.IdCurso == ultimaAulaLiberada.Aula.Curso.Id)
                    .Single().Ordem;
            }

            var curso = item.Turma.Cursos
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