using Quartz;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Repositorios;

namespace AgenteLiberacaoConteudo
{
    internal class LiberacaoAcessoJob : IJob
    {

        private IContexto _contexto;
        public LiberacaoAcessoJob(IContexto contexto)
        {
            _contexto = contexto;
        }

        public void Execute(IJobExecutionContext context)
        {
            RepositorioTurma repo = new RepositorioTurma(_contexto);
            var planejamentos = repo.ListarPlanejamentosNaoConcluidos();
            foreach (var item in planejamentos)
            {  
                item.LiberarAcessosPendentes(_contexto);
            }
        }

      }
}