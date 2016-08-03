using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartLMS.WebUI.Controllers
{
    public class BaseController : Controller
    {
        protected IContexto _contexto;
        public BaseController(IContexto contexto)
        {
            _contexto = contexto;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            ViewBag.AreaConhecimento = Parametro.AREA_CONHECIMENTO;
            ViewBag.Assunto = Parametro.ASSUNTO;
            ViewBag.Curso = Parametro.CURSO;
            ViewBag.Aula = Parametro.AULA;
            ViewBag.AreaConhecimentoPlural = Parametro.AREA_CONHECIMENTO_PLURAL;
        }
    }
}
