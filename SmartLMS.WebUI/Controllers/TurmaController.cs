using Carubbi.Mailer.Implementation;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
        public ActionResult IndexAdmin(string termo, string campoBusca, int pagina = 1)
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
            _contexto.Salvar(_usuarioLogado);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult Create()
        {
            RepositorioCurso cursoRepo = new RepositorioCurso(_contexto);

            ViewBag.Cursos = new SelectList(CursoViewModel.FromEntityList(cursoRepo.ListarCursosAtivos(), 0), "Id", "Nome");
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
                    _contexto.Salvar(_usuarioLogado);
                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de turmas";
                    TempData["Mensagem"] = "Turma criada com sucesso";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de turmas";
                    TempData["Mensagem"] = ex.Message;
                }
            }

            RepositorioCurso cursoRepo = new RepositorioCurso(_contexto);
            ViewBag.Cursos = new SelectList(CursoViewModel.FromEntityList(cursoRepo.ListarCursosAtivos(), 0), "Id", "Nome");
            return View(viewModel);
        }

        private List<Curso> ReordenarCursos(Turma turma)
        {
            RepositorioCurso cursoRepo = new RepositorioCurso(_contexto);

            var cursos = cursoRepo.ListarCursosAtivos();
            var cursosSelecionados = turma.Cursos.Select(c => c.Curso);
            var cursosNaoSelecionados = cursos.Except(cursosSelecionados);
            cursosSelecionados.ToList().ForEach(c =>
                c.Ordem = turma.Cursos.Single(ct => ct.IdCurso == c.Id).Ordem
            );
            var ordemCursosNaoSelecionados = cursosSelecionados.Count() + 1;
            cursosNaoSelecionados.ToList().ForEach(c => c.Ordem = ordemCursosNaoSelecionados++);
            return cursosSelecionados.Union(cursosNaoSelecionados).OrderBy(x => x.Ordem).ToList();
        }

        public ActionResult Edit(Guid id)
        {
            RepositorioTurma repo = new RepositorioTurma(_contexto);

            var turma = repo.ObterPorId(id);

            var cursos = ReordenarCursos(turma);
            ViewBag.Cursos = new SelectList(CursoViewModel.FromEntityList(cursos, 0), "Id", "Nome");

            RepositorioUsuario repoUsu = new RepositorioUsuario(_contexto);
            ViewBag.Alunos = new SelectList(repoUsu.ListarAlunosAtivos(), "id", "Nome");

            return View(TurmaViewModel.FromEntity(turma));
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Guid id, TurmaViewModel viewModel)
        {

            RepositorioTurma repo = new RepositorioTurma(_contexto);
            var turma = repo.ObterPorId(id);
            if (ModelState.IsValid)
            {
                try
                {
                    await repo.AlterarTurma(new SmtpSender(), turma, viewModel.Nome, viewModel.Ativo, viewModel.IdsCursos, viewModel.IdsAlunos, _usuarioLogado);
                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de turmas";
                    TempData["Mensagem"] = "Turma alterada com sucesso";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de turmas";
                    TempData["Mensagem"] = ex.Message;
                }
            }

            var cursos = ReordenarCursos(turma);
            ViewBag.Cursos = new SelectList(CursoViewModel.FromEntityList(cursos, 0), "Id", "Nome");

            RepositorioUsuario repoUsu = new RepositorioUsuario(_contexto);
            ViewBag.Alunos = new SelectList(repoUsu.ListarAlunosAtivos(), "id", "Nome");

            return View(viewModel);

        }


        public ActionResult Planejamentos(Guid idTurma)
        {
            RepositorioTurma repoTurma = new RepositorioTurma(_contexto);
            var turma = repoTurma.ObterPorId(idTurma);
            return View(turma.Cursos.OrderBy(x => x.Ordem).ToList());
        }

       
    }
}