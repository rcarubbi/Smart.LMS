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
    public class AssuntoController : BaseController
    {
        public AssuntoController(IContexto contexto)
            : base(contexto)
        {

        }

        [AllowAnonymous]
        public ActionResult Index(Guid id)
        {
            var areaRepo = new RepositorioAreaConhecimento(_contexto);
            var areas = areaRepo.ListarAreasConhecimento();
            var areaSelecionada = areas.Single(x => x.Id == id);
            if (areaSelecionada == null)
            {
                TempData["TipoMensagem"] = "warning";
                TempData["TituloMensagem"] = "Aviso";
                TempData["Mensagem"] = "Esta área de conhecimento não está disponível no momento";
                return RedirectToAction("Index", "Home");
            }
            AreaConhecimentoViewModel viewModel = AreaConhecimentoViewModel.FromEntity(areaSelecionada, 2);
            ViewBag.OutrasAreas = new SelectList(areas.Except(new List<AreaConhecimento> { areaSelecionada }), "Id", "Nome");
            return View(viewModel);
        }

        [AllowAnonymous]
        public ActionResult ExibirIndiceArea(Guid id)
        {
            var areaRepo = new RepositorioAreaConhecimento(_contexto);
            var area = areaRepo.ObterPorId(id);
            AreaConhecimentoViewModel viewModel = AreaConhecimentoViewModel.FromEntity(area, 2);
            return PartialView("_AreaConhecimentoPanel", viewModel);
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult IndexAdmin(string termo, string campoBusca, int pagina = 1)
        {
            RepositorioAssunto repo = new RepositorioAssunto(_contexto);
            ViewBag.CamposBusca = new SelectList(new string[] { "Nome", "Área de Conhecimento", "Id" });
            return View(AssuntoViewModel.FromEntityList(repo.ListarAssuntos(termo, campoBusca, pagina)));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public ActionResult ListarAssuntos(string termo, string campoBusca, int pagina = 1)
        {
            RepositorioAssunto repo = new RepositorioAssunto(_contexto);
            return Json(AssuntoViewModel.FromEntityList(repo.ListarAssuntos(termo, campoBusca, pagina)));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public ActionResult Excluir(string id)
        {
            RepositorioAssunto repo = new RepositorioAssunto(_contexto);
            repo.Excluir(new Guid(id));
            _contexto.Salvar(_usuarioLogado);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }



        [Authorize(Roles = "Administrador")]
        public ActionResult Create()
        {
            var areaRepo = new RepositorioAreaConhecimento(_contexto);
            var areas = areaRepo.ListarAreasConhecimento();
            ViewBag.Areas = new SelectList(areas, "Id", "Nome");
            return View();
        }


        // POST: professor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public ActionResult Create(AssuntoViewModel assunto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    RepositorioAreaConhecimento repoArea = new RepositorioAreaConhecimento(_contexto);
                    var area =  repoArea.ObterPorId(assunto.IdArea);
                    RepositorioAssunto repo = new RepositorioAssunto(_contexto);
                    repo.Incluir(AssuntoViewModel.ToEntity(assunto, area));
                    _contexto.Salvar(_usuarioLogado);
                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = "Assunto criado com sucesso";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = ex.Message;
                }
            }

            var areaRepo = new RepositorioAreaConhecimento(_contexto);
            var areas = areaRepo.ListarAreasConhecimento();
            ViewBag.Areas = new SelectList(areas, "Id", "Nome");
            return View(assunto);
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(Guid id)
        {
            var areaRepo = new RepositorioAreaConhecimento(_contexto);
            
            var areas = areaRepo.ListarAreasConhecimento();
            ViewBag.Areas = new SelectList(areas, "Id", "Nome");

            var repo = new RepositorioAssunto(_contexto);
            var assunto = repo.ObterPorId(id);
            return View(AssuntoViewModel.FromEntity(assunto, 0));
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public ActionResult Edit(Guid id, AssuntoViewModel viewModel)
        {

            RepositorioAssunto repo = new RepositorioAssunto(_contexto);
            RepositorioAreaConhecimento repoArea = new RepositorioAreaConhecimento(_contexto);

            if (ModelState.IsValid)
            {
                try
                {
                  
                    var area = repoArea.ObterPorId(id);
                    repo.Alterar(AssuntoViewModel.ToEntity(viewModel, area));
                    _contexto.Salvar(_usuarioLogado);
                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = "Assunto alterado com sucesso";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = ex.Message;
                }
            }

            var areas = repoArea.ListarAreasConhecimento();
            ViewBag.Areas = new SelectList(areas, "Id", "Nome");
            return View(viewModel);
        }
    }
}
