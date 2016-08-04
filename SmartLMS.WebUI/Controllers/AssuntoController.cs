using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
    public class AssuntoController : BaseController
    {
        public AssuntoController(IContexto contexto)
            : base(contexto)
        {

        }


        public ActionResult Index(Guid id)
        {
            var areaRepo = new RepositorioAreaConhecimento(_contexto);
            var areas = areaRepo.ListarAreasConhecimento();
            var areaSelecionada = areas.Single(x => x.Id == id);
            AreaConhecimentoViewModel viewModel = AreaConhecimentoViewModel.FromEntity(areaSelecionada, 2);
            ViewBag.OutrasAreas = new SelectList(areas.Except(new List<AreaConhecimento> { areaSelecionada }), "Id", "Nome");
            return View(viewModel);
        }

        public ActionResult ExibirIndiceArea(Guid id)
        {
            var areaRepo = new RepositorioAreaConhecimento(_contexto);
            var area = areaRepo.ObterPorId(id);
            AreaConhecimentoViewModel viewModel = AreaConhecimentoViewModel.FromEntity(area, 2);
            return PartialView("_AreaConhecimentoPanel", viewModel);
        }
    }
}
