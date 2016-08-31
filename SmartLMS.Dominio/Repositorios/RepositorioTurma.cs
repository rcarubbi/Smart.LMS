using SmartLMS.Dominio.Entidades.Liberacao;
using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
