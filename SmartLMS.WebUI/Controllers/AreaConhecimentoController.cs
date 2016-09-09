using SmartLMS.Dominio;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
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

    }
}