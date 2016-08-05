using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Humanizer.DateTimeHumanizeStrategy;
using System.Net;

namespace SmartLMS.WebUI.Controllers
{
    public class AulaController : BaseController
    {
        public AulaController(IContexto contexto)
            : base(contexto)
        {

        }

        [HttpPost]
        public ActionResult AtualizarProgresso(AcessoAulaViewModel viewModel)
        {
            var acessoRepo = new RepositorioAcessoAula(_contexto);
            var usuarioRepo = new RepositorioUsuario(_contexto);
           
            var aulaRepo = new RepositorioAula(_contexto);
            var aula = aulaRepo.ObterAula(viewModel.IdAula, _usuarioLogado.Id);
            acessoRepo.AtualizarProgresso(viewModel.ToEntity(_usuarioLogado, aula.Aula));

            return new HttpStatusCodeResult(HttpStatusCode.OK, "Atualizado");
        }

       

        public ActionResult Ver(Guid id)
        {
            var aulaRepo = new RepositorioAula(_contexto);
            var acessoRepo = new RepositorioAcessoAula(_contexto);
            var aula = aulaRepo.ObterAula(id, _usuarioLogado.Id);
            if (aula.Disponivel)
            {
                acessoRepo.CriarAcesso(new AcessoAula { Aula = aula.Aula, Usuario = _usuarioLogado, DataHoraAcesso = DateTime.Now, Percentual = aula.Percentual, Segundos = aula.Segundos });
                return View(AulaViewModel.FromEntityComArquivos(aula));
            }
            else
            {
                return RedirectToAction("Index", "Aula", new { Id = aula.Aula.Curso.Id });
            }
           
        }


        public ActionResult Index(Guid id)
        {
            var cursoRepo = new RepositorioCurso(_contexto);
            var indice = cursoRepo.ObterIndiceCurso(id, _usuarioLogado.Id);
            var acessoRepo = new RepositorioAcessoAula(_contexto);
            CursoViewModel viewModel = CursoViewModel.FromEntity(indice);
            ViewBag.OutrosCursos = new SelectList(indice.Curso.Assunto.Cursos.Except(new List<Curso> { indice.Curso }), "Id", "Nome");
            return View(viewModel);
        }

        public ActionResult ExibirIndiceCurso(Guid id)
        {
            var cursoRepo = new RepositorioCurso(_contexto);
            var acessoRepo = new RepositorioAcessoAula(_contexto);
            var indice = cursoRepo.ObterIndiceCurso(id, _usuarioLogado.Id);
            CursoViewModel viewModel = CursoViewModel.FromEntity(indice);
            return PartialView("_indiceCurso", viewModel);
        }

        [ChildActionOnly]
        public ActionResult ListarAulas(Guid id, Guid idAulaCorrente)
        {
            var cursoRepo = new RepositorioCurso(_contexto);
            var acessoRepo = new RepositorioAcessoAula(_contexto);
            var indice = cursoRepo.ObterIndiceCurso(id, _usuarioLogado.Id);
            CursoViewModel viewModel = CursoViewModel.FromEntity(indice);
            ViewBag.IdAulaCorrente = idAulaCorrente;
            return PartialView("_listaAulasPequena", viewModel.Aulas);
        }

        [ChildActionOnly]
        public ActionResult ExibirPainelNovasAulas() {
            var aulaRepo = new RepositorioAula(_contexto);
            return PartialView("_PainelNovasAulas", AulaViewModel.FromEntityList(aulaRepo.ListarUltimasAulasAdicionadas(_usuarioLogado.Id), 3,  new DefaultDateTimeHumanizeStrategy()));
        }

        [ChildActionOnly]
        public ActionResult ExibirUltimasAulas()
        {
            var acessoRepo = new RepositorioAcessoAula(_contexto);
            
            return PartialView("_PainelUltimasAulas", AcessoAulaViewModel.FromEntityList(acessoRepo.ListarUltimosAcessos(_usuarioLogado.Id), new DefaultDateTimeHumanizeStrategy()));
        }
    }
}
