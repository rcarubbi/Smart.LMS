using SmartLMS.Dominio;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Web.Mvc;
using System.Linq;
using System.Net;

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
                        select new
                        {
                            DataIngresso = p.DataInicio,
                            Nome = a.Nome,
                            Id = a.Id,
                        };


            return Json(query.ToList());
        }

        [HttpPost]
        public ActionResult Excluir(string id)
        {
            RepositorioTurma turmaRepo = new RepositorioTurma(_contexto);
            turmaRepo.Excluir(new Guid(id));
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult Create()
        {
            RepositorioCurso cursoRepo = new RepositorioCurso(_contexto);

            ViewBag.Cursos = new SelectList(CursoViewModel.FromEntityList(cursoRepo.ListarAtivos(), 0), "Id", "Nome", "NomeAssunto", new { });
            return View();
        }


        [HttpPost]
        public ActionResult Create(TurmaViewModel viewModel)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    RepositorioTurma repo = new RepositorioTurma(_contexto);
                    repo.CriarTurma(viewModel.Nome, viewModel.IdsCursos);
                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de turmas";
                    TempData["Mensagem"] = "Turma criada com sucesso";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de turmas";
                    TempData["Mensagem"] = ex.Message;
                }
            }

            RepositorioCurso cursoRepo = new RepositorioCurso(_contexto);
            ViewBag.Cursos = new SelectList(CursoViewModel.FromEntityList(cursoRepo.ListarAtivos(), 0), "Id", "Nome", "NomeAssunto", new { });
            return View(viewModel);
        }



        public ActionResult Edit(Guid id)
        {
            return View();
        }


        [HttpPost]
        public ActionResult Edit(Guid id, TurmaViewModel viewModel)
        {
            return View();
        }


        public ActionResult Planejamentos(Guid idTurma)
        {
            return View();
        }

    }
}