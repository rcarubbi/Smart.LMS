using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
namespace SmartLMS.WebUI.Controllers
{
    public class AulaController : BaseController
    {
        public AulaController(IContexto contexto)
            : base(contexto)
        {

        }

        public ActionResult Index(Guid id)
        {
            var cursoRepo = new RepositorioCurso(_contexto);
            var curso = cursoRepo.ObterPorId(id);
            CursoViewModel viewModel = CursoViewModel.FromEntity(curso);
            ViewBag.OutrosCursos = new SelectList(curso.Assunto.Cursos.Except(new List<Curso> { curso }), "Id", "Nome");
            return View(viewModel);
        }

        public ActionResult ExibirIndiceCurso(Guid id)
        {
            var cursoRepo = new RepositorioCurso(_contexto);
            var curso = cursoRepo.ObterPorId(id);
            CursoViewModel viewModel = CursoViewModel.FromEntity(curso);
            return PartialView("_indiceCurso", viewModel);
        }
    }
}
