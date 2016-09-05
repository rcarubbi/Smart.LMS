using SmartLMS.Dominio.Entidades.Liberacao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Carubbi.GenericRepository;
using SmartLMS.Dominio.Entidades.Comunicacao;

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

        public void Excluir(Guid id)
        {
            var turma = ObterPorId(id);

            var cursosTurma = _contexto.ObterLista<TurmaCurso>();
            turma.Cursos.ToList().ForEach(a => cursosTurma.Remove(a));

            var aulasPlanejamentos = _contexto.ObterLista<AulaPlanejamento>();
            turma.Planejamentos.SelectMany(x => x.AulasDisponiveis).ToList().ForEach(a => aulasPlanejamentos.Remove(a));

            var avisos = _contexto.ObterLista<Aviso>();
            var planejamentosTurma = turma.Planejamentos.ToList();
            planejamentosTurma.SelectMany(p => p.Avisos).ToList().ForEach(a => avisos.Remove(a));

            var planejamentos = _contexto.ObterLista<Planejamento>();
            turma.Planejamentos.ToList().ForEach(a => planejamentos.Remove(a));

            _contexto.ObterLista<Turma>().Remove(turma);
            _contexto.Salvar();
        }

        public void CriarTurma(string nome, List<Guid> idsCursos)
        {
            
            Turma novaTurma = new Turma();
            novaTurma.Ativo = true;
            novaTurma.DataCriacao = DateTime.Now;
            novaTurma.Nome = nome;
            foreach (var item in idsCursos)
            {
                TurmaCurso tc = new TurmaCurso {
                    Turma = novaTurma,
                    IdCurso = item
                };
                novaTurma.Cursos.Add(tc);
            }
            _contexto.ObterLista<Turma>().Add(novaTurma);
            _contexto.Salvar();
        }
    }
}
