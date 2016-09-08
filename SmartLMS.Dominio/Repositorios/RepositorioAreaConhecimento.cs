using Carubbi.GenericRepository;
using SmartLMS.Dominio.Entidades.Conteudo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioAreaConhecimento
    {
        private IContexto _contexto;
        public RepositorioAreaConhecimento(IContexto contexto)
        {
            _contexto = contexto;
        }

        public IEnumerable<AreaConhecimento> ListarAreasConhecimento()
        {
            return _contexto.ObterLista<AreaConhecimento>().Where(a => a.Ativo).ToList();
        }

        public AreaConhecimento ObterPorId(Guid id)
        {
            return _contexto.ObterLista<AreaConhecimento>().Find(id);
        }

        public PagedListResult<AreaConhecimento> ListarAreasConhecimento(string termo, string campoBusca, int pagina)
        {
            var repo = new GenericRepository<AreaConhecimento>(_contexto);
            var query = new SearchQuery<AreaConhecimento>();
            query.AddFilter(a => (campoBusca == "Nome" && a.Nome.Contains(termo)) ||
                                 (campoBusca == "Id" && a.Id.ToString().Contains(termo)) ||
                                    string.IsNullOrEmpty(campoBusca));

            query.AddSortCriteria(new DynamicFieldSortCriteria<AreaConhecimento>("Nome"));
            query.Take = 8;
            query.Skip = ((pagina - 1) * 8);

            return repo.Search(query);
        }
    }
}
