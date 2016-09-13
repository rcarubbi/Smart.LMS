using Carubbi.GenericRepository;
using SmartLMS.Dominio.Entidades.Conteudo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioCurso
    {

        private IContexto _contexto;
        public RepositorioCurso(IContexto contexto)
        {
            _contexto = contexto;
        }

        public List<Curso> ListarAtivos()
        {
            return _contexto.ObterLista<Curso>().Where(a => a.Ativo).OrderBy(a => a.Nome).ToList();
        }

        public IndiceCurso ObterIndiceCurso(Guid id, Guid? idUsuario)
        {
            var indiceCurso = new IndiceCurso();
            RepositorioAula repoAula = new RepositorioAula(_contexto);
            

            indiceCurso.Curso = _contexto.ObterLista<Curso>().Find(id);
            indiceCurso.AulasInfo = indiceCurso.Curso.Aulas.Where(a => a.Ativo = true)
                .OrderBy(x => x.Ordem)
                .Select(a => new AulaInfo {
                    Aula = a,
                    Disponivel = idUsuario.HasValue ? repoAula.VerificarDisponibilidadeAula(a.Id, idUsuario.Value) : false,
                    Percentual = idUsuario.HasValue ? a.Acessos.Where(x => x.Usuario.Id == idUsuario).LastOrDefault()?.Percentual ?? 0 : 0
            });

            return indiceCurso;
            
        }

        public PagedListResult<Curso> ListarCursos(string termo, string campoBusca, int pagina)
        {
            var repo = new GenericRepository<Curso>(_contexto);
            var query = new SearchQuery<Curso>();
            query.AddFilter(a => (campoBusca == "Nome" && a.Nome.Contains(termo)) ||
                                 (campoBusca == "Id" && a.Id.ToString().Contains(termo)) ||
                                  (campoBusca == "Área de Conhecimento" && a.Assunto.AreaConhecimento.Nome.Contains(termo)) ||
                                  (campoBusca == "Assunto" && a.Assunto.Nome.Contains(termo)) ||
                                    string.IsNullOrEmpty(campoBusca));

            query.AddSortCriteria(new DynamicFieldSortCriteria<Curso>("Assunto.AreaConhecimento.Ordem, Assunto.Ordem, Ordem"));

            query.Take = 8;
            query.Skip = ((pagina - 1) * 8);

            return repo.Search(query);
        }

        public void Excluir(Guid id)
        {
            var curso = ObterPorId(id);
            _contexto.ObterLista<Curso>().Remove(curso);
            _contexto.Salvar();
        }

        public Curso ObterPorId(Guid id)
        {
            return _contexto.ObterLista<Curso>().Find(id);
        }

        public void Incluir(Curso curso)
        {
            curso.DataCriacao = DateTime.Now;
            curso.Ativo = true;
            _contexto.ObterLista<Curso>().Add(curso);
            _contexto.Salvar();
        }

        public void Atualizar(Curso curso)
        {
            var cursoAtual = ObterPorId(curso.Id);
            _contexto.Atualizar(cursoAtual, curso);
            _contexto.Salvar();
        }

        public Curso ObterPorNomeImagem(string nomeImagem)
        {
            return _contexto
                .ObterLista<Curso>()
                .Where(x => x.Imagem == nomeImagem)
                .FirstOrDefault();
        }

        public void Alterar(Curso curso)
        {
            var cursoAtual = ObterPorId(curso.Id);
            curso.DataCriacao = cursoAtual.DataCriacao;
            curso.Aulas = cursoAtual.Aulas;
            _contexto.Atualizar(cursoAtual, curso);
            _contexto.Salvar();
        }
    }
}
