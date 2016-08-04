using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartLMS.WebUI.Controllers
{
    public class BaseController : Controller
    {
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
            
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                _usuarioLogado = usuarioRepo.ObterPorLogin(HttpContext.User.Identity.Name);
                ViewBag.IdUsuarioLogado = _usuarioLogado.Id.ToString();
            }
        }
    }
}
