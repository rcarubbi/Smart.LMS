using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Pessoa;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Transactions;
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
            ImagemUploader uploader = new ImagemUploader();
            var curso = repo.ObterPorId(new Guid(id));
            using (TransactionScope tx = new TransactionScope())
            {
                if (curso.Imagem != null)
                {
                    uploader.DeleteFile(curso.Imagem);
                }
                repo.Excluir(curso.Id);
                tx.Complete();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        [Authorize(Roles = "Administrador")]
        public ActionResult Create()
        {
            var assuntoRepo = new RepositorioAssunto(_contexto);
            var assuntos = assuntoRepo.ListarAssuntosAtivos();
            ViewBag.Assuntos = new SelectList(assuntos, "Id", "Nome");

            var usuarioRepo = new RepositorioUsuario(_contexto);
            var professores = usuarioRepo.ListarProfessoresAtivos();
            ViewBag.Professores = new SelectList(professores, "Id", "Nome");
            return View();
        }


        // POST: professor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public ActionResult Create(CursoViewModel curso)
        {
            var assuntoRepo = new RepositorioAssunto(_contexto);
            var usuarioRepo = new RepositorioUsuario(_contexto);
        
            if (ModelState.IsValid)
            {
                try
                {
                    var assunto = assuntoRepo.ObterPorId(curso.IdAssunto);
                    var professor = (Professor)usuarioRepo.ObterPorId(curso.IdProfessorResponsavel);

                    RepositorioCurso repo = new RepositorioCurso(_contexto);
                    repo.Incluir(CursoViewModel.ToEntity(curso, assunto, professor));

                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = "Curso criado com sucesso";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = ex.Message;
                }
            }

            var assuntos = assuntoRepo.ListarAssuntosAtivos();
            ViewBag.Assuntos = new SelectList(assuntos, "Id", "Nome");
            var professores = usuarioRepo.ListarProfessoresAtivos();
            ViewBag.Professores = new SelectList(professores, "Id", "Nome");
            return View(curso);
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult SalvarImagem()
        {
            
                ImagemUploader uploader = new ImagemUploader();
                RepositorioCurso repo = new RepositorioCurso(_contexto);

               

                var uploadResult = uploader.Upload(Request.Files[0]);
              
                return Json(uploadResult);
           
        }


        [Authorize(Roles = "Administrador")]
        public ActionResult RemoverImagem(string nomeImagem)
        {

            ImagemUploader uploader = new ImagemUploader();

            RepositorioCurso repo = new RepositorioCurso(_contexto);
            Curso curso = repo.ObterPorNomeImagem(nomeImagem);
            if (curso != null)
            {
                curso.Imagem = null;
                repo.Atualizar(curso);
            }
            uploader.DeleteFile(nomeImagem);

            return new HttpStatusCodeResult(HttpStatusCode.OK);

        }


        [Authorize(Roles = "Administrador")]
        public ActionResult RecuperarImagemCurso(string id)
        {
            RepositorioCurso repo = new RepositorioCurso(_contexto);
            var curso = repo.ObterPorId(new Guid(id));

            ImagemUploader uploader = new ImagemUploader();
            var arquivo = uploader.GetFile(curso.Imagem);
            if (arquivo != null)
            {
                return File(arquivo, "image");
            }
            else
            {
                return File(Server.MapPath("~/Content/img/cursos/sem-imagem.png"), "image");
            }
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult ObterDadosImagem(string idCurso)
        {
            RepositorioCurso repo = new RepositorioCurso(_contexto);
            var curso = repo.ObterPorId(new Guid(idCurso));
            ImagemUploader uploader = new ImagemUploader();
            var info = uploader.GetFileInfo(curso.Imagem);
            if (info != null)
            {
                return Json(new { name = curso.Imagem, size = info.Length / 1024 });
            }
            else
            {
                return new EmptyResult();
            }
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(Guid id)
        {
            var assuntoRepo = new RepositorioAssunto(_contexto);
            var assuntos = assuntoRepo.ListarAssuntosAtivos();
            ViewBag.Assuntos = new SelectList(assuntos, "Id", "Nome");

            var usuarioRepo = new RepositorioUsuario(_contexto);
            var professores = usuarioRepo.ListarProfessoresAtivos();
            ViewBag.Professores = new SelectList(professores, "Id", "Nome");

            var repo = new RepositorioCurso(_contexto);
            var curso = repo.ObterPorId(id);
            return View(CursoViewModel.FromEntity(curso, 0));
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public ActionResult Edit(Guid id, CursoViewModel viewModel)
        {

            var assuntoRepo = new RepositorioAssunto(_contexto);
            var usuarioRepo = new RepositorioUsuario(_contexto);
           
            if (ModelState.IsValid)
            {
                try
                {
                    
                    var assunto = assuntoRepo.ObterPorId(viewModel.IdAssunto);
                    var professor = (Professor)usuarioRepo.ObterPorId(viewModel.IdProfessorResponsavel);
                    var repo = new RepositorioCurso(_contexto);
                    repo.Alterar(CursoViewModel.ToEntity(viewModel, assunto, professor));
                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = "Curso alterado com sucesso";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = ex.Message;
                }
            }


            var assuntos = assuntoRepo.ListarAssuntosAtivos();
            ViewBag.Assuntos = new SelectList(assuntos, "Id", "Nome");
            var professores = usuarioRepo.ListarProfessoresAtivos();
            ViewBag.Professores = new SelectList(professores, "Id", "Nome");

            return View(viewModel);
        }
    }


}

