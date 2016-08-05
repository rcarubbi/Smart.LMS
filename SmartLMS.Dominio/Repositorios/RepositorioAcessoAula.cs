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

        private IContexto _contexto;

        public RepositorioAcessoAula(IContexto contexto)
        {

            _contexto = contexto;

        }

        public AcessoAula ObterMaiorPercentual(Guid idAula, Guid idAluno)
        {
            var acessoMaisLongo = _contexto.ObterLista<AcessoAula>()
                    .Where(x => x.Aula.Id == idAula && x.Usuario.Id == idAluno)
                    .OrderByDescending(x => x.Percentual)
                    .FirstOrDefault();
            return acessoMaisLongo;
        }

        public void AtualizarProgresso(AcessoAula acesso)
        {
            var acessos = _contexto.ObterLista<AcessoAula>();
            var acessoExistente = acessos.Where(x => x.Aula.Id == acesso.Aula.Id && x.Usuario.Id == acesso.Usuario.Id).ToList().LastOrDefault();
            if (acessoExistente == null)
            {
                acessos.Add(acesso);
            }
            else
            {
                acesso.Id = acessoExistente.Id;
                _contexto.Atualizar(acessoExistente, acesso);
            }
            _contexto.Salvar();
        }

        public void CriarAcesso(AcessoAula acessoAula)
        {
            var acessos = _contexto.ObterLista<AcessoAula>();
            acessos.Add(acessoAula);
            _contexto.Salvar();
        }

        public IEnumerable<AcessoAula> ListarUltimosAcessos(Guid id)
        {
            var query = from acesso in _contexto.ObterLista<AcessoAula>()
            where acesso.Usuario.Id == id
            group acesso by acesso.Aula.Id
            into acessoPorAula
            select acessoPorAula.OrderByDescending(x => x.DataHoraAcesso).FirstOrDefault();

            return query.ToList();
        }
    }
}
