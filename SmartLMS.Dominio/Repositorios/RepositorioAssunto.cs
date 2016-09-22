using Carubbi.GenericRepository;
using SmartLMS.Dominio.Entidades.Conteudo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioAssunto
    {
        private IContexto _contexto;
        public RepositorioAssunto(IContexto contexto)
        {
            _contexto = contexto;
        }

        public Assunto ObterPorId(Guid id)
        {
            return _contexto.ObterLista<Assunto>().Find(id);
        }

        public PagedListResult<Assunto> ListarAssuntos(string termo, string campoBusca, int pagina)
        {
            var repo = new GenericRepository<Assunto>(_contexto);
            var query = new SearchQuery<Assunto>();
            query.AddFilter(a => (campoBusca == "Nome" && a.Nome.Contains(termo)) ||
                                 (campoBusca == "Id" && a.Id.ToString().Contains(termo)) ||
                                 (campoBusca == "Área de Conhecimento" && a.AreaConhecimento.Nome.Contains(termo)) ||
                                    string.IsNullOrEmpty(campoBusca));

            query.AddSortCriteria(new DynamicFieldSortCriteria<Assunto>("AreaConhecimento.Ordem, Ordem"));

            query.Take = 8;
            query.Skip = ((pagina - 1) * 8);

            return repo.Search(query);
        }

        public void Excluir(Guid id)
        {
            var assunto = ObterPorId(id);
            _contexto.ObterLista<Assunto>().Remove(assunto);
          
        }

        public void Incluir(Assunto assunto)
        {
            assunto.Ativo = true;
            assunto.DataCriacao = DateTime.Now;
            _contexto.ObterLista<Assunto>().Add(assunto);
           
        }

        public void Alterar(Assunto assunto)
        {
            var assuntoAtual = ObterPorId(assunto.Id);
            assunto.DataCriacao = assuntoAtual.DataCriacao;
            assunto.Cursos = assuntoAtual.Cursos;
            _contexto.Atualizar(assuntoAtual, assunto);


            if (!assunto.Ativo)
            {
                foreach (var curso in assunto.Cursos)
                {
                    RepositorioCurso repoCurso = new RepositorioCurso(_contexto);
                    curso.Ativo = false;
                    repoCurso.Alterar(curso);
                }
            }
        }

        public List<Assunto> ListarAssuntosAtivos()
        {
            return _contexto
                .ObterLista<Assunto>()
                .Where(x => x.Ativo)
                .OrderBy(x => x.Nome)
                .ToList();
        }

   
    }
}
