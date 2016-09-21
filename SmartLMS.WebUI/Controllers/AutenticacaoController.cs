using Carubbi.Mailer.Implementation;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades.Pessoa;
using SmartLMS.Dominio.Servicos;
using SmartLMS.WebUI.Models;
using SmartLMS.WebUI.Servicos;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

namespace SmartLMS.WebUI.Controllers
{
    [AllowAnonymous]
    public class AutenticacaoController : BaseController
    {
        public AutenticacaoController(IContexto contexto)
            : base(contexto)
        {

        }

        // GET: Autenticacao
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel viewModel)
        {

            if (ModelState.IsValid)
            {
                ServicoAutenticacao autenticacao = new ServicoAutenticacao(_contexto, new SmtpSender());
                if (autenticacao.Login(viewModel.Login, viewModel.Senha))
                {
                    FormsAuthentication.SetAuthCookie(viewModel.Login, viewModel.LembrarMe);
                    string url = FormsAuthentication.GetRedirectUrl(viewModel.Login, viewModel.LembrarMe);
                    return Json(new { Url = url, Autenticado = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.AddModelError("UsuarioOuSenhaInvalidos", "Usuário ou Senha inválidos");
                }
            }
            return Json(new { ValidationSummary = RenderRazorViewToString("_errosValidacao", viewModel), Autenticado = false }, JsonRequestBehavior.AllowGet);
      
        }

        public ActionResult EsqueciMinhaSenha()
        {
            EsqueciMinhaSenhaViewModel viewModel = new EsqueciMinhaSenhaViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> EsqueciMinhaSenha(EsqueciMinhaSenhaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var sender = new SmtpSender();
                ServicoAutenticacao servico = new ServicoAutenticacao(_contexto, sender);
                var senha = servico.RecuperarSenha(viewModel.Email);
                if (senha != null)
                {
                    
                    ServicoNotificacao servicoNotificacao = new ServicoNotificacao(_contexto, sender);
                    await Task.Run(() => servicoNotificacao.NotificarRecuperacaoSenha(viewModel.Email, senha, this.Url.Action("Login", "Autenticacao"))).ConfigureAwait(false);
                    ViewBag.Mensagem = "Enviamos um e-mail para você com a sua senha.";
                }
                else
                {
                    ViewBag.Mensagem = "";
                    ModelState.AddModelError("EmailNaoEncontrado", "E-mail não encontrado");
                }
            }
            return View(viewModel);
        }

        [Authorize]
        public ActionResult Sair()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult AlterarSenha()
        {
            AlterarSenhaViewModel viewModel = new AlterarSenhaViewModel();
            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AlterarSenha(AlterarSenhaViewModel novaSenha)
        {
            if (ModelState.IsValid)
            {
                
                ServicoAutenticacao autenticacao = new ServicoAutenticacao(_contexto, new SmtpSender());
                autenticacao.AlterarUsuario(_usuarioLogado.Id, _usuarioLogado.Nome, _usuarioLogado.Email, _usuarioLogado.Login, novaSenha.Senha, _usuarioLogado.Ativo, (Perfil)Enum.Parse(typeof(Perfil), _contexto.UnProxy(_usuarioLogado).GetType().Name));
                TempData["TipoMensagem"] = "success";
                TempData["TituloMensagem"] = "Notificação";
                TempData["Mensagem"] = "Senha alterada com sucesso!";
                return RedirectToAction("Index", "Home");
            }
            return View(novaSenha);
        }
    }
}
