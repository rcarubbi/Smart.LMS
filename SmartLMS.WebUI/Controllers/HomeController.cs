using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.Dominio.Servicos;
using SmartLMS.WebUI.Models;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        ServicoBuscaContextual servicoBusca;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            servicoBusca = new ServicoBuscaContextual(_contexto, usuarioRepo.ObterPorLogin(HttpContext.User.Identity.Name));
        }

        public HomeController(IContexto contexto)
            : base(contexto)
        {
            
        }

        public ActionResult Index()
        {
            RepositorioAreaConhecimento areaRepo = new RepositorioAreaConhecimento(_contexto);
            ConteudoPrincipalViewModel viewModel = new ConteudoPrincipalViewModel();
            viewModel.TituloAulasAssistidas = Parametro.TITULO_AULAS_ASSISTIDAS;
            viewModel.TituloUltimosCursos = Parametro.TITULO_ULTIMOS_CURSOS;
            viewModel.TituloUltimasAulas = Parametro.TITULO_ULTIMAS_AULAS;
            viewModel.AreasConhecimento = AreaConhecimentoViewModel.FromEntityList(areaRepo.ListarAreasConhecimento());
            return View(viewModel);
        }


        public ActionResult BuscaContextual(string term) {
           
            var resultados = servicoBusca.Pesquisar(term).Entities.Select(r => new { label = r.Descricao }).ToList();
            return Json(resultados, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BuscaContextualPaginada(string termo, int pagina)
        {
            var resultados = servicoBusca.Pesquisar(termo, pagina);
            return Json(resultados, JsonRequestBehavior.AllowGet);
        }
    }
}