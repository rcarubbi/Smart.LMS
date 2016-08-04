using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Humanizer.DateTimeHumanizeStrategy;

namespace SmartLMS.WebUI.Controllers
{
    public class AulaController : BaseController
    {
        public AulaController(IContexto contexto)
            : base(contexto)
        {

        }


        public ActionResult ReproduzirVideoVimeo(Guid id)
        {
            var aulaRepo = new RepositorioAula(_contexto);
            var aula = aulaRepo.ObterAula(id, _usuarioLogado.Id);
            if (aula.Disponivel)
                return Redirect(string.Format("http://player.vimeo.com/video/{0}", aula.Aula.Conteudo));
            else
                return new EmptyResult();
        }

        public ActionResult Ver(Guid id)
        {
            var aulaRepo = new RepositorioAula(_contexto);
            var aula = aulaRepo.ObterAula(id, _usuarioLogado.Id);
            return View(AulaViewModel.FromEntityComArquivos(aula));
            //ViewBag.OutrasAulas = new SelectList(indice.Curso.Assunto.Cursos.Except(new List<Curso> { indice.Curso }), "Id", "Nome");
        }


        public ActionResult Index(Guid id)
        {
            var cursoRepo = new RepositorioCurso(_contexto);
            var indice = cursoRepo.ObterIndiceCurso(id, _usuarioLogado.Id);
            var acessoRepo = new RepositorioAcessoAula(_contexto, _usuarioLogado.Id);
            CursoViewModel viewModel = CursoViewModel.FromEntity(indice);
            ViewBag.OutrosCursos = new SelectList(indice.Curso.Assunto.Cursos.Except(new List<Curso> { indice.Curso }), "Id", "Nome");
            return View(viewModel);
        }

        public ActionResult ExibirIndiceCurso(Guid id)
        {
            var cursoRepo = new RepositorioCurso(_contexto);
            var acessoRepo = new RepositorioAcessoAula(_contexto, _usuarioLogado.Id);
            var indice = cursoRepo.ObterIndiceCurso(id, _usuarioLogado.Id);
            CursoViewModel viewModel = CursoViewModel.FromEntity(indice);
            return PartialView("_indiceCurso", viewModel);
        }

        [ChildActionOnly]
        public ActionResult ExibirPainelNovasAulas() {
            var aulaRepo = new RepositorioAula(_contexto);
            return PartialView("_PainelNovasAulas", AulaViewModel.FromEntityList(aulaRepo.ListarUltimasAulasAdicionadas(_usuarioLogado.Id), 3,  new DefaultDateTimeHumanizeStrategy()));
        }

    }
}
