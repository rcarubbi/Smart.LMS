using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Entidades.Pessoa;
using SmartLMS.Dominio.Repositorios;
using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartLMS.WebUI.Controllers
{
    public class BaseController : Controller
    {

        protected override void OnException(ExceptionContext filterContext)
        {
    
            if (!Request.IsAjaxRequest())
            {
                base.OnException(filterContext);
                filterContext.ExceptionHandled = true;
                Exception e = filterContext.Exception;
                ViewData["Exception"] = e; // pass the exception to the view
                filterContext.Result = View("Error");
            }
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        protected Usuario _usuarioLogado;
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
            ViewBag.AssuntoPlural = Parametro.ASSUNTO_PLURAL;
            ViewBag.CursoPlural = Parametro.CURSO_PLURAL;
            ViewBag.AulaPlural = Parametro.AULA_PLURAL;

            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                _usuarioLogado = usuarioRepo.ObterPorLogin(HttpContext.User.Identity.Name);
                ViewBag.IdUsuarioLogado = _usuarioLogado != null? _usuarioLogado.Id.ToString() : string.Empty;
            }
        }
    }
}
