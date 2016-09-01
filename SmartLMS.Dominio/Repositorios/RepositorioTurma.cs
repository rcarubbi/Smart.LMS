using SmartLMS.Dominio.Entidades.Liberacao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Carubbi.GenericRepository;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioTurma
    {
        private IContexto _contexto;
        public RepositorioTurma(IContexto contexto)
        {
            _contexto = contexto;
        }

        public List<Planejamento> ListarPlanejamentosNaoConcluidos()
        {
            return _contexto.ObterLista<Planejamento>().Where(p => !p.Concluido).ToList();
        }

        public IEnumerable ListarTurmas()
        {
            return _contexto.ObterLista<Turma>()
                .Where(a => a.Ativo)
                .OrderBy(a => a.Nome)
                .ToList();
        }

        public PagedListResult<Turma> ListarTurmas(string termo, string campoBusca, int pagina)
        {
           
            
            var repo = new GenericRepository<Turma>(_contexto);
            var query = new SearchQuery<Turma>();
            query.AddFilter(a => (campoBusca == "Nome" && a.Nome.Contains(termo)) ||
                                 (campoBusca == "Curso" && a.Cursos.Any(c => c.Curso.Nome.Contains(termo))) ||
                                    string.IsNullOrEmpty(campoBusca));

            query.AddSortCriteria(new DynamicFieldSortCriteria<Turma>("Nome"));
            query.Take = 10;
            query.Skip = ((pagina - 1) * 10);

            return repo.Search(query);
        }

        public Turma ObterPorId(Guid id)
        {
            return _contexto.ObterLista<Turma>().Find(id);
        }
    }
}
