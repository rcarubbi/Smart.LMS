using System;
using System.Collections.Generic;
using SmartLMS.Dominio.Entidades;
using System.Linq;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioAula
    {
        private IContexto _contexto;

        public RepositorioAula(IContexto contexto)
        {
            _contexto = contexto;
        }

        public IEnumerable<Aula> ListarUltimasAulasAdicionadas(Guid idAluno)
        {
            return _contexto.ObterLista<TurmaAluno>().Where(t => t.IdAluno == idAluno)
                .SelectMany(x => x.Turma.AulasDisponiveis)
                .OrderByDescending(x => x.DataInclusao).Take(6);
        }

        internal AulaInfo ObterAula(Guid id, Guid idUsuario)
        {
            var aula = _contexto.ObterLista<Aula>().Find(id);
            
            return new AulaInfo
            {
                Aula = _contexto.ObterLista<Aula>().Find(id),
                Disponivel = aula.Turmas.Any(t => t.Alunos.Any(al => al.IdAluno == idUsuario))
            };
        }
    }
}