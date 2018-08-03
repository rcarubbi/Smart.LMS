using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SmartLMS.DAL;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Repositories;

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
            
            using (var context = new Context())
            {
                var parameterRepository = new ParameterRepository(context);
                Parameter.APP_NAME = parameterRepository.ObterValorPorChave(Parameter.APP_NAME_KEY);

                Parameter.KNOWLEDGE_AREA_PLURAL = parameterRepository.ObterValorPorChave(Parameter.KNOWLEDGE_AREA_PLURAL_KEY);
                Parameter.KNOWLEDGE_AREA = parameterRepository.ObterValorPorChave(Parameter.KNOWLEDGE_AREA_KEY);
                
                Parameter.COURSE_PLURAL = parameterRepository.ObterValorPorChave(Parameter.COURSE_PLURAL_KEY);
                Parameter.COURSE= parameterRepository.ObterValorPorChave(Parameter.COURSE_KEY);
                
                Parameter.SUBJECT_PLURAL= parameterRepository.ObterValorPorChave(Parameter.SUBJECT_PLURAL_KEY);
                Parameter.SUBJECT= parameterRepository.ObterValorPorChave(Parameter.SUBJECT_KEY);
                
                Parameter.CLASS= parameterRepository.ObterValorPorChave(Parameter.CLASS_KEY);
                Parameter.CLASS_PLURAL = parameterRepository.ObterValorPorChave(Parameter.CLASS_PLURAL_KEY);
                Parameter.FILE= parameterRepository.ObterValorPorChave(Parameter.FILE_KEY);
                Parameter.WATCHED_CLASSES_TITLE= parameterRepository.ObterValorPorChave(Parameter.WATCHED_CLASSES_TITLE_KEY);
                Parameter.FILE_STORAGE = parameterRepository.ObterValorPorChave(Parameter.FILE_STORAGE_KEY);
                Parameter.LAST_CLASSES_TITLE= parameterRepository.ObterValorPorChave(Parameter.LAST_CLASSES_TITLE_KEY);
                Parameter.DELIVERED_CLASS_NOTICE_BODY= parameterRepository.ObterValorPorChave(Parameter.DELIVERED_CLASS_NOTICE_BODY_KEY);
            }
                

            
        }
    }
}
