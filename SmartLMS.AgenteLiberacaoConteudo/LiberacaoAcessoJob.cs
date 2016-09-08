using Carubbi.Mailer.Implementation;
using NLog;
using Quartz;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Repositorios;
using System;

namespace AgenteLiberacaoConteudo
{
    public class LiberacaoAcessoJob : IJob
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IContexto _contexto;
        public LiberacaoAcessoJob(IContexto contexto)
        {
            _contexto = contexto;
        }

        public void Execute(IJobExecutionContext context)
        {
            logger.Trace("Ciclo de execução iniciado");
            RepositorioTurma repo = new RepositorioTurma(_contexto);
            var planejamentos = repo.ListarPlanejamentosNaoConcluidos();
            foreach (var item in planejamentos)
            {
                try
                {
                    logger.Info($"Liberando acessos para o planejamento do dia {item.DataInicio.ToShortDateString()} da turma {item.Turma.Nome}");
                    item.LiberarAcessosPendentes(_contexto, new SmtpSender());
                    logger.Info($"Acessos liberados");
                }
                catch (Exception ex)
                {
                   logger.Error(ex, "Erro ao liberar acessos");
                }
            }
            logger.Trace("Ciclo de execução finalizado");
        }

      }
}