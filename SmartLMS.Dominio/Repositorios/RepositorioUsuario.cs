using SmartLMS.Dominio.Entidades;
using System.Linq;
using System;
using System.Collections.Generic;
using Carubbi.GenericRepository;

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

        public Usuario ObterPorId(Guid id)
        {
            return _contexto.ObterLista<Usuario>().Find(id);
        }
        internal void Salvar(Usuario usuario)
        {
            _contexto.ObterLista<Usuario>().Add(usuario);
            _contexto.Salvar();
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
            query.Take = 10;
            query.Skip = ((pagina - 1) * 10);

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
            var turmas = _contexto.ObterLista<TurmaAluno>();
            var avisos = _contexto.ObterLista<Aviso>();

            aluno.Avisos.ToList().ForEach(a => usuarioAvisos.Remove(a));
            aluno.AcessosAula.ToList().ForEach(a => acessosAula.Remove(a));
            aluno.AcessosArquivo.ToList().ForEach(a => acessosArquivo.Remove(a));
            aluno.Turmas.ToList().ForEach(a => turmas.Remove(a));
            avisos.Where(a => a.Usuario.Id == id).ToList().ForEach(a => avisos.Remove(a));

            _contexto.ObterLista<Aluno>().Remove(aluno);
            _contexto.Salvar();
        }

     
    }
}
