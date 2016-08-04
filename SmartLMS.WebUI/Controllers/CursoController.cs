using SmartLMS.Dominio;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using SmartLMS.Dominio.Entidades;

namespace SmartLMS.WebUI.Controllers
{
    public class CursoController : BaseController
    {
        public CursoController(IContexto contexto)
            : base(contexto)
        {

        }

        public ActionResult ExibirIndiceAssunto(Guid id)
        {
            var assuntoRepo = new RepositorioAssunto(_contexto);
            var assunto = assuntoRepo.ObterPorId(id);
            AssuntoViewModel viewModel = AssuntoViewModel.FromEntity(assunto, 3);
            return PartialView("_indiceAssunto", viewModel);
        }

        public ActionResult Index(Guid id)
        {
            var assuntoRepo = new RepositorioAssunto(_contexto);
            var assunto = assuntoRepo.ObterPorId(id);
            AssuntoViewModel viewModel = AssuntoViewModel.FromEntity(assunto, 3);
            ViewBag.OutrosAssuntos = new SelectList(assunto.AreaConhecimento.Assuntos.Except(new List<Assunto> { assunto }), "Id", "Nome");
            return View(viewModel);
        }
    }
}
