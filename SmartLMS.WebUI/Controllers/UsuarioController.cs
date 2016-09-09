using Carubbi.Utils.Data;
using Carubbi.Utils.DataTypes;
using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.Dominio.Servicos;
using SmartLMS.WebUI.Models;
using System;
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

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExibirCabecalho()
        {
            var usuarioRepo = new RepositorioUsuario(_contexto);
            var areaConhecimentoRepo = new RepositorioAreaConhecimento(_contexto);
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                CabecalhoViewModel viewModel = new CabecalhoViewModel
                {
                    Usuario = UsuarioViewModel.FromEntity(usuarioRepo.ObterPorLogin(HttpContext.User.Identity.Name)),
                    AreasConhecimento = AreaConhecimentoViewModel.FromEntityList(areaConhecimentoRepo.ListarAreasConhecimento(), 2).ToList(),
                };

                return PartialView($"_ExibirCabecalho{viewModel.Usuario.NomePerfil}", viewModel);
            }
            else
            {
                CabecalhoViewModel viewModel = new CabecalhoViewModel
                {
                    AreasConhecimento = AreaConhecimentoViewModel.FromEntityList(areaConhecimentoRepo.ListarAreasConhecimento(), 2).ToList(),
                };
                return PartialView("_Login", viewModel);
            }
                
        }


        public ActionResult Historico()
        {
            TipoAcesso tipo = TipoAcesso.Arquivo;
            ViewBag.TiposAcesso = new SelectList(tipo.ToDataSource<TipoAcesso>(),"Key", "Value");
            var periodo = new DateRange();
            periodo.StartDate = DateTime.Now.AddMonths(-1);
            periodo.EndDate = DateTime.Now;
            ServicoHistorico servico = new ServicoHistorico(_contexto, new DefaultDateTimeHumanizeStrategy());
            return View(AcessoViewModel.FromEntityList(servico.ListarHistorico(periodo, 1, _usuarioLogado.Id, TipoAcesso.Todos)));
        }



        public ActionResult ListarHistorico(DateTime? dataInicio, DateTime? dataFim, TipoAcesso tipo = TipoAcesso.Todos, int pagina = 1)
        {
            var periodo = new DateRange();
            periodo.StartDate = dataInicio;
            periodo.EndDate = dataFim;

            ServicoHistorico servico = new ServicoHistorico(_contexto, new DefaultDateTimeHumanizeStrategy());
            return Json(AcessoViewModel.FromEntityList(servico.ListarHistorico(periodo, pagina, _usuarioLogado.Id, tipo)), JsonRequestBehavior.AllowGet);
        }
      

    }
}
