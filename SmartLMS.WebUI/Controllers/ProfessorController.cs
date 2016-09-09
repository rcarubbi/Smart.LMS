using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades.Pessoa;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.Dominio.Servicos;
using SmartLMS.WebUI.Models;
using System;
using System.Net;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProfessorController : BaseController
    {
        public ProfessorController(IContexto contexto)
            : base(contexto)
        {

        }

        [HttpPost]
        public ActionResult ListarProfessores(string termo, string campoBusca, int pagina = 1)
        {
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            var professores = usuarioRepo.ListarProfessores(termo, campoBusca, pagina);
            return Json(UsuarioViewModel.FromEntityList(professores));
        }

        [HttpPost]
        public ActionResult Excluir(string id)
        {

            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            usuarioRepo.ExcluirProfessor(new Guid(id));
            return new HttpStatusCodeResult(HttpStatusCode.OK);


        }

        // GET: professor
        public ActionResult IndexAdmin(string termo, string campoBusca, int pagina = 1)
        {
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);
            ViewBag.CamposBusca = new SelectList(new string[] { "Nome", "Email", "Id" });
            return View(UsuarioViewModel.FromEntityList(usuarioRepo.ListarProfessores(termo, campoBusca, pagina)));
        }



        // GET: professor/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: professor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UsuarioViewModel professor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ServicoAutenticacao servicoAuth = new ServicoAutenticacao(_contexto);
                    servicoAuth.CriarUsuario(professor.Nome, professor.Login, professor.Email, professor.Senha, Perfil.Professor);

                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de professores";
                    TempData["Mensagem"] = "Professor criado com sucesso";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de professores";
                    TempData["Mensagem"] = ex.Message;
                }


            }

            return View(professor);
        }

        // GET: Aluno/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Professor professor = _contexto.ObterLista<Professor>().Find(id);

            if (professor == null)
            {
                return HttpNotFound();
            }
            return View(UsuarioViewModel.FromEntity(professor));
        }

        // POST: professor/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nome,Login,Senha,Email,Ativo")] Professor professor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ServicoAutenticacao servicoAuth = new ServicoAutenticacao(_contexto);
                    servicoAuth.AlterarUsuario(professor.Id, professor.Nome, professor.Email, professor.Login, professor.Senha, professor.Ativo, Perfil.Professor);

                    TempData["TipoMensagem"] = "success";
                    TempData["TituloMensagem"] = "Administração de professores";
                    TempData["Mensagem"] = "Professor alterado com sucesso";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["TipoMensagem"] = "error";
                    TempData["TituloMensagem"] = "Administração de professores";
                    TempData["Mensagem"] = ex.Message;
                }
            }
            return View(professor);
        }

        // GET: professor/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Professor professor = _contexto.ObterLista<Professor>().Find(id);


            if (professor == null)
            {
                return HttpNotFound();
            }
            return View(professor);
        }

        // POST: professor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {


            var usuarioRepo = new RepositorioUsuario(_contexto);
            usuarioRepo.ExcluirProfessor(id);

            TempData["TipoMensagem"] = "error";
            TempData["TituloMensagem"] = "Administração de professores";
            TempData["Mensagem"] = "Professor excluído com sucesso";

            return RedirectToAction("IndexAdmin");
        }


    }
}
