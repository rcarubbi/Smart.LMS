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
            var usuarioRepo = new RepositorioUsuario(_contexto);
            var areaConhecimentoRepo = new RepositorioAreaConhecimento(_contexto);

            CabecalhoViewModel viewModel = new CabecalhoViewModel
            {
                Usuario = UsuarioViewModel.FromEntity(usuarioRepo.ObterPorLogin(HttpContext.User.Identity.Name)),
                AreasConhecimento = AreaConhecimentoViewModel.FromEntityList(areaConhecimentoRepo.ListarAreasConhecimento(), 2).ToList(),
            };

            return PartialView($"_ExibirCabecalho{viewModel.Usuario.NomePerfil}", viewModel);
        }

     


      

    }
}
