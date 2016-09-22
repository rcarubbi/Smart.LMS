using Carubbi.GenericRepository;
using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Entidades.Historico;
using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Entidades.Pessoa;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioUsuario
    {
        private IContexto _contexto;

        public RepositorioUsuario(IContexto contexto)
        {
            _contexto = contexto;
        }

        public Usuario ObterPorEmail(string login)
        {
            return _contexto.ObterLista<Usuario>().FirstOrDefault(u => u.Email == login);
        }

        internal ICollection<Turma> ListarTurmas(Guid idUsuario)
        {
            var usuario = ObterPorId(idUsuario);
            var lista = new List<Turma>();
            if (usuario is Aluno)
            {
                ((Aluno)usuario).Planejamentos.Select(x => x.Turma).ToList().ForEach(x => lista.Add(_contexto.UnProxy(x)));

            }
            else if (usuario is Aluno)
            {
                _contexto.ObterLista<Turma>()
                    .Where(t =>
                            t.Cursos.Select(x => x.Curso.ProfessorResponsavel).Any(p => p.Id == idUsuario)
                            || t.Cursos.SelectMany(x => x.Curso.Aulas).Select(x => x.Professor).Any(x => x.Id == idUsuario)
                    ).ToList().ForEach(x => lista.Add(_contexto.UnProxy(x)));

            }

            return lista;
        }

        public Usuario ObterPorId(Guid id)
        {
            return _contexto.ObterLista<Usuario>().Find(id);
        }

        internal void Salvar(Usuario usuario)
        {
            _contexto.ObterLista<Usuario>().Add(usuario);
            
        }

        public PagedListResult<Professor> ListarProfessores(string termo, string campoBusca, int pagina)
        {
            var repo = new GenericRepository<Professor>(_contexto);
            var query = new SearchQuery<Professor>();
            query.AddFilter(a => (campoBusca == "Nome" && a.Nome.Contains(termo)) ||
                                             (campoBusca == "Email" && a.Email.Contains(termo)) ||
                                             (campoBusca == "Id" && a.Id.ToString().Contains(termo)) ||
                                             string.IsNullOrEmpty(campoBusca));

            query.AddSortCriteria(new DynamicFieldSortCriteria<Professor>("Nome"));
            query.Take = 8;
            query.Skip = ((pagina - 1) * 8);

            return repo.Search(query);
        }

        public void ExcluirProfessor(Guid id, Usuario usuarioLogado)
        {
            Professor professor = _contexto.ObterLista<Professor>().Find(id);
            var usuarioAvisos = _contexto.ObterLista<UsuarioAviso>();
            var avisos = _contexto.ObterLista<Aviso>();

            professor.AvisosVistos.ToList().ForEach(a => usuarioAvisos.Remove(a));
            avisos.Where(a => a.Usuario.Id == id).ToList().ForEach(a => avisos.Remove(a));
           
            _contexto.ObterLista<Professor>().Remove(professor);

            try
            {
                _contexto.Salvar(usuarioLogado);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException.InnerException.Message.Contains("FK_dbo.Aula_dbo.Usuario_Professor_Id"))
                {
                    throw new ApplicationException("Este professor possui aulas associadas a ele");
                }
                else if (ex.InnerException.InnerException.Message.Contains("FK_dbo.Curso_dbo.Usuario_ProfessorResponsavel_Id"))
                {
                    throw new ApplicationException("Este professor possui cursos sob sua responsabilidade");
                }
            }
        }

        public List<Professor> ListarProfessoresAtivos()
        {
            return _contexto.ObterLista<Professor>().Where(x => x.Ativo).OrderBy(x => x.Nome).ToList();
        }

       

        public PagedListResult<Aluno> ListarAlunos(string termo, string campoBusca, int pagina)
        {
            var repo = new GenericRepository<Aluno>(_contexto);
            var query = new SearchQuery<Aluno>();
            query.AddFilter(a => (campoBusca == "Nome" && a.Nome.Contains(termo)) ||
                                             (campoBusca == "Email" && a.Email.Contains(termo)) ||
                                             (campoBusca == "Id" && a.Id.ToString().Contains(termo)) ||
                                             string.IsNullOrEmpty(campoBusca));

            query.AddSortCriteria(new DynamicFieldSortCriteria<Aluno>("Nome"));
            query.Take = 8;
            query.Skip = ((pagina - 1) * 8);

            return repo.Search(query);
        }

        public Usuario ObterPorLogin(string login)
        {
            return _contexto.ObterLista<Usuario>().SingleOrDefault(u => u.Login == login);
        }

        public List<Usuario> ListarPorPerfil(Perfil roleName)
        {
            return _contexto.ObterLista<Usuario>().Where(x => x.GetType().Name == roleName.ToString()).ToList();
        }

        public void ExcluirAluno(Guid id)
        {
            Aluno aluno = _contexto.ObterLista<Aluno>().Find(id);
            var usuarioAvisos = _contexto.ObterLista<UsuarioAviso>();
            var acessosAula = _contexto.ObterLista<AcessoAula>();
            var acessosArquivo = _contexto.ObterLista<AcessoArquivo>();
            var avisos = _contexto.ObterLista<Aviso>();
            var planejamentos = _contexto.ObterLista<Planejamento>().Where(x => x.Alunos.Any(y => y.Id == id));
            var comentariosAluno = _contexto.ObterLista<Comentario>().Where(x => x.Usuario.Id == id);
            var comentarios = _contexto.ObterLista<Comentario>();

            aluno.AvisosVistos.ToList().ForEach(a => usuarioAvisos.Remove(a));
            aluno.AcessosAula.ToList().ForEach(a => acessosAula.Remove(a));
            aluno.AcessosArquivo.ToList().ForEach(a => acessosArquivo.Remove(a));
            comentariosAluno.ToList().ForEach(a => comentarios.Remove(a));
            avisos.Where(a => a.Usuario.Id == id).ToList().ForEach(a => avisos.Remove(a));
            planejamentos.ToList().ForEach(x => x.Alunos.Remove(aluno));
            
            _contexto.ObterLista<Aluno>().Remove(aluno);
        
        }

        public List<Aluno> ListarAlunosAtivos()
        {
            return _contexto.ObterLista<Aluno>().Where(x => x.Ativo).OrderBy(x => x.Nome).ToList();
        }

    
    }
}
