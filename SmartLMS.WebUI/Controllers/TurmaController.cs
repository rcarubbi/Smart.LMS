using SmartLMS.Dominio;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Web.Mvc;
using System.Linq;
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
        public ActionResult Index(string termo, string campoBusca, int pagina = 1)
        {
            RepositorioTurma turmaRepo = new RepositorioTurma(_contexto);
            ViewBag.CamposBusca = new SelectList(new string[] { "Nome", "Curso" });
            return View(TurmaViewModel.FromEntityList(turmaRepo.ListarTurmas(termo, campoBusca, pagina)));
        }



        [HttpPost]
        public ActionResult ListarTurmas(string termo, string campoBusca, int pagina = 1)
        {
            RepositorioTurma turmaRepo = new RepositorioTurma(_contexto);
            return Json(TurmaViewModel.FromEntityList(turmaRepo.ListarTurmas(termo, campoBusca, pagina)));
        }


        [HttpPost]
        public ActionResult ListarCursos(Guid id)
        {
            RepositorioTurma turmaRepo = new RepositorioTurma(_contexto);
            var turma = turmaRepo.ObterPorId(id);
            
            return Json(CursoViewModel.FromEntityList(turma.Cursos.ToList()));
        }


        [HttpPost]
        public ActionResult ListarAlunos(Guid id)
        {
            RepositorioTurma turmaRepo = new RepositorioTurma(_contexto);
            var turma = turmaRepo.ObterPorId(id);

            var query = from p in turma.Planejamentos
                        from a in p.Alunos
                        orderby p.DataInicio descending, a.Nome
                        select new {
                            DataIngresso = p.DataInicio,
                            Nome = a.Nome,
                            Id = a.Id,
                        };


            return Json(query.ToList());
        }

    }
}