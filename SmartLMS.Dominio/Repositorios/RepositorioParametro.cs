using SmartLMS.Dominio.Entidades;
using System.Linq;
using System;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioParametro
    {
        private IContexto _contexto;
        public RepositorioParametro(IContexto contexto)
        {
            _contexto = contexto;
        }

        public string ObterValorPorChave(string chave)
        {
            return _contexto.ObterLista<Parametro>().Single(p => p.Chave == chave).Valor;
        }

    
    }
}
