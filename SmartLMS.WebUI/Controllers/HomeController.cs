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
            servicoBusca = new ServicoBuscaContextual(_contexto, _usuarioLogado);
        }

        public HomeController(IContexto contexto)
            : base(contexto)
        {
            
        }

        public ActionResult Index()
        {
            var areaRepo = new RepositorioAreaConhecimento(_contexto);
            var viewModel = AreaConhecimentoViewModel.FromEntityList(areaRepo.ListarAreasConhecimento(), 2);
          
            TempData["TituloAulasAssistidas"] = Parametro.TITULO_AULAS_ASSISTIDAS;
            TempData["TituloUltimasAulas"] = Parametro.TITULO_ULTIMAS_AULAS;
            return View(viewModel);
        }


        public ActionResult BuscaContextual(string term) {
           
            var resultados = servicoBusca.Pesquisar(term).Entities.Select(r => new { label = r.Descricao }).ToList();
            return Json(resultados, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BuscaContextualPaginada(string termo, int pagina)
        {
            var resultados = servicoBusca.Pesquisar(termo, pagina);
            return Json(new { paginaCorrente = pagina, resultados = resultados }, JsonRequestBehavior.AllowGet);
        }
    }
}