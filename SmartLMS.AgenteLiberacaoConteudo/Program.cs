using SmartLMS.DAL;
using SmartLMS.Dominio;
using StructureMap;
using Topshelf;
using Topshelf.StructureMap;
using Topshelf.Quartz.StructureMap;
using Quartz;

namespace AgenteLiberacaoConteudo
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(serviceConfig =>
            {
                serviceConfig.UseNLog();

                var container = new Container(cfg =>
                {
                    cfg.For<IContexto>().Use<Contexto>();
                });

                serviceConfig.UseStructureMap(container);

                serviceConfig.Service<ServicoLiberacaoConteudo>(serviceInstance =>
                        {
                            serviceInstance.ConstructUsingStructureMap();
                            serviceInstance.WhenStarted(execute => execute.Iniciar());
                            serviceInstance.WhenStopped(execute => execute.Parar());

                            serviceInstance.UseQuartzStructureMap();

                            serviceInstance.ScheduleQuartzJob(q =>
                                q.WithJob(() =>
                                    JobBuilder.Create<LiberacaoAcessoJob>().Build())
                                    .AddTrigger(() =>
                                        TriggerBuilder.Create()
                                            .WithSimpleSchedule(builder => builder
                                                                            .WithIntervalInHours(6)
                                                                            .RepeatForever())
                                                                            .Build())
                                );

                        });

                serviceConfig.EnableServiceRecovery(recoveryOption =>
                {
                    recoveryOption.RestartService(5);
                });

                serviceConfig.SetServiceName("SmartLMSAgenteLiberacaoConteudo");
                serviceConfig.SetDisplayName("Smart LMS - Agente de Liberação de Conteudo");
                serviceConfig.SetDescription("Agente responsável pela liberação de novas aulas para os alunos");
                serviceConfig.StartAutomatically();
            });
        }
    }
}
