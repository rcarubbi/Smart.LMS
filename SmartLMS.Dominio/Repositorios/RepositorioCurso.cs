using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartLMS.Dominio.Entidades;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioCurso
    {

        private IContexto _contexto;
        public RepositorioCurso(IContexto contexto)
        {
            _contexto = contexto;
        }

        public Curso ObterPorId(Guid id)
        {
            return _contexto.ObterLista<Curso>().Find(id);
        }
    }
}
