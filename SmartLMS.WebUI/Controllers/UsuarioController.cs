using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize]
    public class UsuarioController : BaseController
    {
        public UsuarioController(IContexto contexto)
             : base(contexto)
        {

        }


        [ChildActionOnly]
        public ActionResult ExibirCabecalho()
        {
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            RepositorioAreaConhecimento areaConhecimentoRepo = new RepositorioAreaConhecimento(_contexto);

            CabecalhoViewModel viewModel = new CabecalhoViewModel
            {
                Usuario = UsuarioViewModel.FromEntity(usuarioRepo.ObterPorLogin(HttpContext.User.Identity.Name)),
                AreasConhecimento = AreaConhecimentoViewModel.FromEntityList(areaConhecimentoRepo.ListarAreasConhecimento(), 2).ToList(),
            };

            return PartialView($"_ExibirCabecalho{viewModel.Usuario.NomePerfil}", viewModel);
        }

        [ChildActionOnly]
        public ActionResult ExibirUltimasAulas()
        {
          

            return PartialView("_PainelUltimasAulas", new List<AulaViewModel>());
        }


        [ChildActionOnly]
        public ActionResult ExibirAvisos()
        {
            var repo = new RepositorioAviso(_contexto);
            var dateTimeHumanizerStrategy = new DefaultDateTimeHumanizeStrategy();
            return PartialView("_PainelAvisos", AvisoViewModel.FromEntityList(repo.ListarAvisosNaoVistos(_usuarioLogado.Id), dateTimeHumanizerStrategy));
        }

    }
}
