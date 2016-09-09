using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Historico;
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
                TempData["TipoMensagem"] = "warning";
                TempData["TituloMensagem"] = "Aviso";
                TempData["Mensagem"] = "Esta aula não está disponível para você";
                return RedirectToAction("Index", "Aula", new { Id = aula.Aula.Curso.Id });
            }
           
        }

        [AllowAnonymous]
        public ActionResult Index(Guid id)
        {
            var cursoRepo = new RepositorioCurso(_contexto);
            var indice = cursoRepo.ObterIndiceCurso(id, _usuarioLogado?.Id);
            var acessoRepo = new RepositorioAcessoAula(_contexto);
            CursoViewModel viewModel = CursoViewModel.FromEntity(indice);
            ViewBag.OutrosCursos = new SelectList(indice.Curso.Assunto.Cursos.Except(new List<Curso> { indice.Curso }), "Id", "Nome");
            return View(viewModel);
        }

        [AllowAnonymous]
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
            return PartialView("_PainelNovasAulas", AulaViewModel.FromEntityList(aulaRepo.ListarUltimasAulasLiberadas(_usuarioLogado.Id), new DefaultDateTimeHumanizeStrategy()));
        }

     
        [ChildActionOnly]
        public ActionResult ExibirUltimasAulas()
        {
            var acessoRepo = new RepositorioAcessoAula(_contexto);
            return PartialView("_PainelUltimasAulas", AcessoAulaViewModel.FromEntityList(acessoRepo.ListarUltimosAcessos(_usuarioLogado.Id), new DefaultDateTimeHumanizeStrategy()));
        }

        [HttpPost]
        public ActionResult ListarComentarios(Guid idAula, int pagina = 1)
        {
            var aulaRepo = new RepositorioAula(_contexto);
            var aula = aulaRepo.ObterAula(idAula, _usuarioLogado.Id);
            var humanizer = new DefaultDateTimeHumanizeStrategy();
            var comentarios = aula.Aula.Comentarios
                .OrderByDescending(x => x.DataHora)
                .Skip(((pagina - 1) * 10))
                .Take(10)
                .ToList();

            return Json(ComentarioViewModel.FromEntityList(comentarios, humanizer, _usuarioLogado.Id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Comentar(FormCollection formData)
        {
            if (!string.IsNullOrEmpty(formData["Comentario"]))
            {
                ComentarioViewModel comentario = new ComentarioViewModel
                {
                    IdAula = new Guid(formData["IdAula"]),
                    Comentario = formData["Comentario"]
                };

                var aulaRepo = new RepositorioAula(_contexto);
                var aula = aulaRepo.ObterAula(comentario.IdAula, _usuarioLogado.Id);
                comentario.DataHora = DateTime.Now;
                aulaRepo.Comentar(comentario.ToEntity(_usuarioLogado, aula.Aula));
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpPost]
      
        public ActionResult ExcluirComentario(long idComentario)
        {
                var aulaRepo = new RepositorioAula(_contexto);
             
                aulaRepo.ExcluirComentario(idComentario);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        public ActionResult Baixar(Guid id)
        {

            RepositorioAula aulaRepo = new RepositorioAula(_contexto);
            Arquivo arquivo = aulaRepo.ObterArquivo(id);
            aulaRepo.GravarAcesso(arquivo, _usuarioLogado);
            return File(Url.Content("~/" + SmartLMS.Dominio.Entidades.Parametro.STORAGE_ARQUIVOS + "/" + arquivo.ArquivoFisico), "application/octet-stream", arquivo.ArquivoFisico);
        }

        public ActionResult IndexAdmin(string termo, string campoBusca, int pagina = 1)
        {
            return View();
        }

    }
}
