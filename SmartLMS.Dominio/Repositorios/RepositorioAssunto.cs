using SmartLMS.Dominio.Entidades;
using System;

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
    }
}
