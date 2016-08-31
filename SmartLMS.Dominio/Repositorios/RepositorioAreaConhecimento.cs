using SmartLMS.Dominio.Entidades.Conteudo;
using System;
using System.Collections.Generic;
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
            return _contexto.ObterLista<AreaConhecimento>().Where(a => a.Ativo).ToList();
        }

        public AreaConhecimento ObterPorId(Guid id)
        {
            return _contexto.ObterLista<AreaConhecimento>().Find(id);
        }
    }
}
