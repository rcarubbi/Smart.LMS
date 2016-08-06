using Carubbi.GenericRepository;
using Carubbi.Utils.Data;
using SmartLMS.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Dominio.Servicos
{
    public class ServicoBuscaContextual
    {
        private IContexto _contexto;
        private Usuario _usuarioLogado;
        public ServicoBuscaContextual(IContexto contexto, Usuario usuarioLogado)
        {
            _contexto = contexto;
            _usuarioLogado = usuarioLogado;
        }

        public PagedListResult<ResultadoBusca> Pesquisar(string termo, int pagina = 1, int quantidade = 8)
        {
            var query = Pesquisar<AreaConhecimento>(termo, TipoResultado.AreaConhecimento).Union(
                Pesquisar<Assunto>(termo, TipoResultado.Assunto).Union(
                    Pesquisar<Curso>(termo, TipoResultado.Curso).Union(
                    Pesquisar<Aula>(termo, TipoResultado.Aula).Union(
                    Pesquisar<Arquivo>(termo, TipoResultado.Arquivo))))).OrderBy(x => x.Descricao);

            GenericRepository<ResultadoBusca> repo = new GenericRepository<ResultadoBusca>(_contexto, query);
            SearchQuery<ResultadoBusca> filter = new SearchQuery<ResultadoBusca>();
            filter.Take = quantidade;
            filter.Skip = ((pagina - 1) * quantidade);
            filter.SortCriterias.Add(new DynamicFieldSortCriteria<ResultadoBusca>("Descricao"));


            return repo.Search(filter);  
        }

        private IQueryable<ResultadoBusca> Pesquisar<T>(string termo, TipoResultado tipo)
            where T : class, IResultadoBusca
        {

            return _contexto.ObterLista<T>().Where(x => x.Ativo == true && x.Nome.Contains(termo))
                .Select(x => new ResultadoBusca
                {
                    Id = x.Id,
                    Descricao = x.Nome,
                    Tipo = tipo
                });
        }

     

   

   
    }
}
