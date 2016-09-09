using Carubbi.GenericRepository;
using SmartLMS.Dominio.Entidades.Conteudo;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
            return _contexto.ObterLista<AreaConhecimento>().Where(a => a.Ativo).OrderBy(x => x.Ordem).ToList();
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

            query.AddSortCriteria(new DynamicFieldSortCriteria<AreaConhecimento>("Ordem"));
            query.Take = 8;
            query.Skip = ((pagina - 1) * 8);

            return repo.Search(query);
        }

        public void ExcluirAreaConhecimento(Guid id)
        {
            var area = _contexto.ObterLista<AreaConhecimento>().Find(id);
            _contexto.ObterLista<AreaConhecimento>().Remove(area);
            _contexto.Salvar();
        }

        public void Incluir(AreaConhecimento areaConhecimento)
        {
            areaConhecimento.Ativo = true;
            areaConhecimento.DataCriacao = DateTime.Now;
            _contexto.ObterLista<AreaConhecimento>().Add(areaConhecimento);
            _contexto.Salvar();
        }

        public void Alterar(AreaConhecimento areaConhecimento)
        {
            var areaConhecimentoAtual = ObterPorId(areaConhecimento.Id);
            areaConhecimento.Assuntos = areaConhecimentoAtual.Assuntos;
            areaConhecimento.DataCriacao = areaConhecimentoAtual.DataCriacao;
            _contexto.Atualizar(areaConhecimentoAtual, areaConhecimento);
            _contexto.Salvar();
        }
    }
}

