using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Entidades.Pessoa;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.Dominio.Servicos;
using SmartLMS.WebUI.Models;
using System;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web.Mvc;
namespace SmartLMS.WebUI.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AlunoController : BaseController
    {
        public AlunoController(IContexto contexto)
            : base(contexto)
        {

        }

        [HttpPost]
        public ActionResult ListarAlunos(string termo, string campoBusca, int pagina = 1)
        {
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            return Json(UsuarioViewModel.FromEntityList(usuarioRepo.ListarAlunos(termo, campoBusca, pagina)));
        }

        [HttpPost]
        public ActionResult Excluir(string id)
        {

            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            usuarioRepo.ExcluirAluno(new Guid(id));
            return new HttpStatusCodeResult(HttpStatusCode.OK);


        }

        // GET: Aluno
        public ActionResult Index(string termo, string campoBusca, int pagina = 1)
        {
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            ViewBag.CamposBusca = new SelectList(new string[] { "Nome", "Email", "Id" });
            return View(UsuarioViewModel.FromEntityList(usuarioRepo.ListarAlunos(termo, campoBusca, pagina)));
        }



        // GET: Aluno/Create
        public ActionResult Create()
        {
            RepositorioTurma repo = new RepositorioTurma(_contexto);
            ViewBag.Turmas = new SelectList(repo.ListarTurmas(), "Id", "Nome");
            return View();
        }

        // POST: Aluno/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UsuarioViewModel aluno)
        {
            RepositorioTurma repo = new RepositorioTurma(_contexto);
            if (ModelState.IsValid)
            {
                try
                {
                    using (TransactionScope tx = new TransactionScope())
                    {

                        ServicoAutenticacao servicoAuth = new ServicoAutenticacao(_contexto);
                        var novoAluno = servicoAuth.CriarUsuario(aluno.Nome, aluno.Login, aluno.Email, aluno.Senha, Perfil.Aluno);

                        var turma = repo.ObterPorId(aluno.Turma);

                        var planejamento = turma.Planejamentos.SingleOrDefault(x => x.DataInicio.Date == DateTime.Today);
                        if (planejamento == null)
                        {
                            planejamento = new Planejamento
                            {
                                DataInicio = DateTime.Today,
                                Turma = turma
                            };
                            turma.Planejamentos.Add(planejamento);
                            
                        }

                        planejamento.Alunos.Add((Aluno)novoAluno);

                        _contexto.Salvar();
                        tx.Complete();
                    }
                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de alunos";
                    TempData["Mensagem"] = "Aluno criado com sucesso";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de alunos";
                    TempData["Mensagem"] = ex.Message;
                }
            }

            
            ViewBag.Turmas = new SelectList(repo.ListarTurmas(), "Id", "Nome");
            return View(aluno);
        }

        // GET: Aluno/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aluno aluno = _contexto.ObterLista<Aluno>().Find(id);

            if (aluno == null)
            {
                return HttpNotFound();
            }
            return View(UsuarioViewModel.FromEntity(aluno));
        }

        // POST: Aluno/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nome,Login,Senha,Email,Ativo")] Aluno aluno)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ServicoAutenticacao servicoAuth = new ServicoAutenticacao(_contexto);
                    servicoAuth.AlterarUsuario(aluno.Id, aluno.Nome, aluno.Email, aluno.Login, aluno.Senha, aluno.Ativo, Perfil.Aluno);

                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de alunos";
                    TempData["Mensagem"] = "Aluno alterado com sucesso";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de alunos";
                    TempData["Mensagem"] = ex.Message;
                }
            }
            return View(aluno);
        }

        // GET: Aluno/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aluno aluno = _contexto.ObterLista<Aluno>().Find(id);


            if (aluno == null)
            {
                return HttpNotFound();
            }
            return View(aluno);
        }

        // POST: Aluno/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {


            var usuarioRepo = new RepositorioUsuario(_contexto);
            usuarioRepo.ExcluirAluno(id);

            TempData["TipoMensagem"] = "error";
            TempData["TituloMensagem"] = "Administração de alunos";
            TempData["Mensagem"] = "Aluno excluído com sucesso";

            return RedirectToAction("Index");
        }


    }
}
