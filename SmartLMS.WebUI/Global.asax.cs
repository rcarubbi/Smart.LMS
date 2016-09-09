using SmartLMS.DAL;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SmartLMS.WebUI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            using (var contexto = new Contexto())
            {
                RepositorioParametro parametroRepo = new RepositorioParametro(contexto);
                Parametro.PROJETO = parametroRepo.ObterValorPorChave(Parametro.NOME_PROJETO);

                Parametro.AREA_CONHECIMENTO_PLURAL = parametroRepo.ObterValorPorChave(Parametro.CHAVE_AREA_CONHECIMENTO_PLURAL);
                Parametro.AREA_CONHECIMENTO = parametroRepo.ObterValorPorChave(Parametro.CHAVE_AREA_CONHECIMENTO);

                Parametro.CURSO_PLURAL = parametroRepo.ObterValorPorChave(Parametro.CHAVE_CURSO_PLURAL);
                Parametro.CURSO = parametroRepo.ObterValorPorChave(Parametro.CHAVE_CURSO);

                Parametro.ASSUNTO_PLURAL = parametroRepo.ObterValorPorChave(Parametro.CHAVE_ASSUNTO_PLURAL);
                Parametro.ASSUNTO= parametroRepo.ObterValorPorChave(Parametro.CHAVE_ASSUNTO);

                Parametro.AULA= parametroRepo.ObterValorPorChave(Parametro.CHAVE_AULA);
                Parametro.AULA_PLURAL = parametroRepo.ObterValorPorChave(Parametro.CHAVE_AULA_PLURAL);
                Parametro.ARQUIVO= parametroRepo.ObterValorPorChave(Parametro.CHAVE_ARQUIVO);
                Parametro.TITULO_AULAS_ASSISTIDAS = parametroRepo.ObterValorPorChave(Parametro.CHAVE_TITULO_AULAS_ASSISTIDAS);
                Parametro.STORAGE_ARQUIVOS = parametroRepo.ObterValorPorChave(Parametro.CHAVE_STORAGE_ARQUIVOS);
                Parametro.TITULO_ULTIMAS_AULAS = parametroRepo.ObterValorPorChave(Parametro.CHAVE_TITULO_ULTIMAS_AULAS);
                Parametro.CORPO_NOTIFICACAO_AULA_LIBERADA = parametroRepo.ObterValorPorChave(Parametro.CHAVE_CORPO_NOTIFICACAO_AULA_LIBERADA);
            }
                

            
        }
    }
}
