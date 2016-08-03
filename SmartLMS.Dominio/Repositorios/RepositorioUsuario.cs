using SmartLMS.Dominio.Entidades;
using System.Linq;
using System;
using System.Collections.Generic;

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
            return _contexto.ObterLista<Usuario>().FirstOrDefault(u => u.Login == login);
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

        public Usuario ObterPorLogin(string login)
        {
            return _contexto.ObterLista<Usuario>().SingleOrDefault(u => u.Login == login);
        }

        public List<Usuario> ListarPorPerfil(Perfil roleName)
        {
            return _contexto.ObterLista<Usuario>().Where(x => x.GetType().Name == roleName.ToString()).ToList();
        }
    }
}
