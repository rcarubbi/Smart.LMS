using SmartLMS.Dominio;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class TurmaController : BaseController
    {

        public TurmaController(IContexto contexto)
            : base(contexto)
        {

        }

        // GET: Turma
        public ActionResult Index()
        {

            return View();
        }
    }
}