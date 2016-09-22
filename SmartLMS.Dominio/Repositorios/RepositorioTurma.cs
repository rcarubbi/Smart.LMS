using Carubbi.GenericRepository;
using Carubbi.Mailer.Interfaces;
using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Entidades.Pessoa;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

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
            query.Take = 8;
            query.Skip = ((pagina - 1) * 8);

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
          
        }

        public void CriarTurma(string nome, List<Guid> idsCursos)
        {

            Turma novaTurma = new Turma();
            novaTurma.Ativo = true;
            novaTurma.DataCriacao = DateTime.Now;
            novaTurma.Nome = nome;
            var ordem = 1;
            foreach (var item in idsCursos)
            {
                TurmaCurso tc = new TurmaCurso
                {
                    Turma = novaTurma,
                    IdCurso = item,
                    Ordem = ordem++
                };
                novaTurma.Cursos.Add(tc);
            }
            _contexto.ObterLista<Turma>().Add(novaTurma);
          
        }

        public async Task AlterarTurma(IMailSender sender, Turma turma, string nome, bool ativo, List<Guid> idsCursos, List<Guid> idsAlunos, Usuario usuarioLogado)
        {
            using (TransactionScope tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var turmaAlterada = new Turma
                {
                    Id = turma.Id,
                    Nome = nome,
                    Ativo = ativo,
                    DataCriacao = turma.DataCriacao,
                    Cursos = turma.Cursos,
                    Planejamentos = turma.Planejamentos
                };

                _contexto.Atualizar(turma, turmaAlterada);
                RemoverCursos(turma, idsCursos);
                AtualizarCursos(turma, idsCursos);
                RemoverAlunos(turma, idsAlunos);
                AdicionarNovosAlunos(sender, turma, idsAlunos, usuarioLogado);
                _contexto.Salvar(usuarioLogado);

                await turma.SincronizarAcessosAync(_contexto, sender);
                _contexto.Salvar(usuarioLogado);
                tx.Complete();
            }
        }

        private void AdicionarNovosAlunos(IMailSender sender, Turma turma, List<Guid> idsAlunos, Usuario usuarioLogado)
        {
            if (idsAlunos == null)
                return;

            var planejamentoDoDia = turma.Planejamentos.FirstOrDefault(x => x.DataInicio == DateTime.Today);
            if (planejamentoDoDia == null)
            {
                planejamentoDoDia = new Planejamento
                {
                    DataInicio = DateTime.Today,
                    Turma = turma

                };
                _contexto.ObterLista<Planejamento>().Add(planejamentoDoDia);
                _contexto.Salvar(usuarioLogado);
            }
            var idsAlunosExistentes = turma.Planejamentos.SelectMany(x => x.Alunos).Select(x => x.Id);

            var idsAlunosNovos = idsAlunos.Except(idsAlunosExistentes);
            var novosAlunos = _contexto.ObterLista<Aluno>().Where(a => idsAlunosNovos.Contains(a.Id)).ToList();
            foreach (var item in novosAlunos)
            {
                planejamentoDoDia.Alunos.Add(item);
            }
            _contexto.Atualizar(planejamentoDoDia, planejamentoDoDia);

            // Enviar emails das aulas já disponibilizadas no dia para os novos alunos
            foreach (var aula in planejamentoDoDia.AulasDisponiveis)
            {
                planejamentoDoDia.EnviarEmailsLiberacaoAula(_contexto, sender, aula.Aula, novosAlunos);
            }

            planejamentoDoDia.LiberarAcessosPendentes(_contexto, sender);
        }

        private void RemoverAlunos(Turma turma, List<Guid> idsAlunos)
        {
            var idsAtuais = turma.Planejamentos.SelectMany(x => x.Alunos).Select(x => x.Id).ToList();
            if (idsAlunos == null)
                idsAlunos = new List<Guid>();

            var alunosAExcluir = idsAtuais.Except(idsAlunos).ToList();

            foreach (var item in _contexto.ObterLista<Aluno>().Where(a => alunosAExcluir.Contains(a.Id)).ToList())
            {
                foreach (var planejamento in item.Planejamentos.Where(p => p.Turma.Id == turma.Id).ToList())
                {
                    planejamento.Alunos.Remove(item);
                }
            }
        }

        private void AtualizarCursos(Turma turma, List<Guid> idsCursos)
        {


            var cursos = _contexto.ObterLista<Curso>().Where(x => idsCursos.Contains(x.Id));
            foreach (var item in cursos)
            {

                var curso = turma.Cursos.FirstOrDefault(x => x.IdCurso == item.Id);
                if (curso != null)
                {
                    curso.Ordem = idsCursos.IndexOf(item.Id) + 1;
                    _contexto.Atualizar<TurmaCurso>(curso, curso);
                }
                else
                {

                    turma.Cursos.Add(new TurmaCurso
                    {
                        Turma = turma,
                        Curso = item,
                        Ordem = idsCursos.IndexOf(item.Id) + 1
                    });

                    turma.Planejamentos.ToList().ForEach(x => x.Concluido = false);
                }
            }


        }

        private void RemoverCursos(Turma turma, List<Guid> idsCursos)
        {
            var idsAtuais = turma.Cursos.Select(x => x.IdCurso);
            var cursosAExcluir = idsAtuais.Except(idsCursos).ToList();


            foreach (var item in cursosAExcluir)
            {
                var tc = _contexto.ObterLista<TurmaCurso>().First(x => x.IdTurma == turma.Id && x.IdCurso == item);
                var aulasCurso = tc.Curso.Aulas.Select(x => x.Id);
                RemoverAcessos(tc.Turma.Planejamentos, aulasCurso);
                turma.Cursos.Remove(tc);
            }
        }

        private void RemoverAcessos(ICollection<Planejamento> planejamentos, IEnumerable<Guid> idsAulas)
        {
            foreach (var item in planejamentos)
            {
                var aulas = item.AulasDisponiveis.Where(a => idsAulas.Contains(a.IdAula));
                aulas.ToList().ForEach(a => item.AulasDisponiveis.Remove(a));
            }
        }
    }
}
