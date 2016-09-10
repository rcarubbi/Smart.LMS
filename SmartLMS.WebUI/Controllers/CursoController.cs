using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize]
    public class CursoController : BaseController
    {
        public CursoController(IContexto contexto)
            : base(contexto)
        {

        }

        [AllowAnonymous]
        public ActionResult ExibirIndiceAssunto(Guid id)
        {
            var assuntoRepo = new RepositorioAssunto(_contexto);
            var assunto = assuntoRepo.ObterPorId(id);
            AssuntoViewModel viewModel = AssuntoViewModel.FromEntity(assunto, 3);
            return PartialView("_indiceAssunto", viewModel);
        }

        [AllowAnonymous]
        public ActionResult Index(Guid id)
        {
            var assuntoRepo = new RepositorioAssunto(_contexto);
            var assunto = assuntoRepo.ObterPorId(id);
            AssuntoViewModel viewModel = AssuntoViewModel.FromEntity(assunto, 3);
            ViewBag.OutrosAssuntos = new SelectList(assunto.AreaConhecimento.Assuntos.Except(new List<Assunto> { assunto }), "Id", "Nome");
            return View(viewModel);
        }


        [Authorize(Roles = "Administrador")]
        public ActionResult IndexAdmin(string termo, string campoBusca, int pagina = 1)
        {
            ViewBag.CamposBusca = new SelectList(new string[] { "Nome", "Assunto", "Área de Conhecimento", "Id" });
            RepositorioCurso repo = new RepositorioCurso(_contexto);
            return View(CursoViewModel.FromEntityList(repo.ListarCursos(termo, campoBusca, pagina)));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public ActionResult ListarCursos(string termo, string campoBusca, int pagina = 1)
        {
            RepositorioCurso repo = new RepositorioCurso(_contexto);
            return Json(CursoViewModel.FromEntityList(repo.ListarCursos(termo, campoBusca, pagina)));
        }


        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public ActionResult Excluir(string id)
        {
            RepositorioCurso repo = new RepositorioCurso(_contexto);
            repo.Excluir(new Guid(id));
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
