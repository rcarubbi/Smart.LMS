using SmartLMS.Dominio;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Net;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize(Roles ="Administrador")]
    public class AreaConhecimentoController : BaseController
    {
        public AreaConhecimentoController(IContexto contexto)
            : base(contexto)
        {

        }

        // GET: AreaConhecimento
        public ActionResult IndexAdmin(string termo, string campoBusca, int pagina = 1)
        {
            RepositorioAreaConhecimento repo = new RepositorioAreaConhecimento(_contexto);
            ViewBag.CamposBusca = new SelectList(new string[] { "Nome", "Id" });
            return View(AreaConhecimentoViewModel.FromEntityList(repo.ListarAreasConhecimento(termo, campoBusca, pagina)));
        }

        [HttpPost]
        public ActionResult ListarAreasConhecimento(string termo, string campoBusca, int pagina = 1)
        {
            RepositorioAreaConhecimento repo = new RepositorioAreaConhecimento(_contexto);
            return Json(AreaConhecimentoViewModel.FromEntityList(repo.ListarAreasConhecimento(termo, campoBusca, pagina)));
        }

        [HttpPost]
        public ActionResult Excluir(string id)
        {

            RepositorioAreaConhecimento repo = new RepositorioAreaConhecimento(_contexto);
            repo.ExcluirAreaConhecimento(new Guid(id));
            return new HttpStatusCodeResult(HttpStatusCode.OK);


        }

        public ActionResult Create()
        {
            return View();
        }


        // POST: professor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AreaConhecimentoViewModel areaConhecimento)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    RepositorioAreaConhecimento repo = new RepositorioAreaConhecimento(_contexto);
                    repo.Incluir(AreaConhecimentoViewModel.ToEntity(areaConhecimento));
                    _contexto.Salvar(_usuarioLogado);
                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = "Área de conhecimento criada com sucesso";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = ex.Message;
                }


            }

            return View(areaConhecimento);
        }

        public ActionResult Edit(Guid id)
        {
            RepositorioAreaConhecimento repo = new RepositorioAreaConhecimento(_contexto);
            var area = repo.ObterPorId(id);
            return View(AreaConhecimentoViewModel.FromEntity(area, 0));
        }

        [HttpPost]
        public ActionResult Edit(Guid id, AreaConhecimentoViewModel viewModel)
        {

            RepositorioAreaConhecimento repo = new RepositorioAreaConhecimento(_contexto);
            if (ModelState.IsValid)
            {
                try
                {
                    repo.Alterar(AreaConhecimentoViewModel.ToEntity(viewModel));
                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = "Área de conhecimento alterada com sucesso";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de conteúdo";
                    TempData["Mensagem"] = ex.Message;
                }
            }


            return View(viewModel);

        }

    }
}