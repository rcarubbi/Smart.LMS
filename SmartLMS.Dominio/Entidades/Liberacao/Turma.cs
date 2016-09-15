using Carubbi.Mailer.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SmartLMS.Dominio.Entidades.Liberacao
{
    public class Turma : Entidade
    {
        public Turma()
        {
            Cursos = new List<TurmaCurso>();
            Planejamentos = new List<Planejamento>();
        }
        public string Nome { get; set; }

        public virtual ICollection<TurmaCurso> Cursos { get; set; }

        public virtual ICollection<Planejamento> Planejamentos { get; set; }

        internal async Task SincronizarAcessosAync(IContexto contexto, IMailSender sender)
        {
            await Task.Run(() => { 
                using (TransactionScope tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var planejamento in Planejamentos)
                    {
                        AulaPlanejamento ultimaAulaLiberada = planejamento.AulasDisponiveis.OrderByDescending(x => x.DataLiberacao).FirstOrDefault();
                        if (ultimaAulaLiberada != null)
                        {
                            var ordemCurso = Cursos.Single(c => c.IdCurso == ultimaAulaLiberada.Aula.Curso.Id).Ordem;
                            var aulasCursosAnteriores = Cursos.Where(c => c.Ordem < ordemCurso).SelectMany(x => x.Curso.Aulas);
                            var aulasCursosAnterioresNaoDisponibilizadas = aulasCursosAnteriores.Except(planejamento.AulasDisponiveis.Select(x => x.Aula));
                            foreach (var item in aulasCursosAnterioresNaoDisponibilizadas)
                            {
                                planejamento.DisponibilizarAula(contexto, sender, item);
                            }
                        }

                    }
                    tx.Complete();
                }
            }).ConfigureAwait(false);
        }
    }
}
