using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Entidades.Pessoa;

namespace SmartLMS.Dominio
{
    public interface IContexto : Carubbi.GenericRepository.IDbContext
    {
        void Salvar();

        void Salvar(Usuario usuario);

        void Atualizar<TEntidade>(TEntidade objetoAntigo, TEntidade objetoNovo) where TEntidade : class;

        IDbSet<TEntidade> ObterLista<TEntidade>() where TEntidade : class;
        void ConfigurarParaApi();

        T UnProxy<T>(T proxyObject) where T : class;
        void Recarregar<T>(T entidade) where T : class;
    }
}
