using SmartLMS.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Dominio.Repositorios
{
    public class RepositorioAcessoAula
    {
        private Guid _idAluno;
        private IContexto _contexto;

        public RepositorioAcessoAula(IContexto contexto, Guid idAluno)
        {
            _idAluno = idAluno;
            _contexto = contexto;

        }

        public AcessoAula ObterMaiorPercentual(Guid idAula)
        {
            var acessoMaisLongo = _contexto.ObterLista<AcessoAula>()
                    .Where(x => x.Aula.Id == idAula && x.Aluno.Id == _idAluno)
                    .OrderByDescending(x => x.Percentual)
                    .FirstOrDefault();
            return acessoMaisLongo;
        }
    }
}
