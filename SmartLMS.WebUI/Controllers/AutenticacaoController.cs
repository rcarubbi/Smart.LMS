using SmartLMS.Dominio;
using SmartLMS.Dominio.Servicos;
using SmartLMS.WebUI.Models;
using SmartLMS.WebUI.Servicos;
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
        public ActionResult Login(LoginViewModel viewModel)
        {

            if (ModelState.IsValid)
            {
                ServicoAutenticacao autenticacao = new ServicoAutenticacao(_contexto);
                if (autenticacao.Login(viewModel.Login, viewModel.Senha))
                {
                    FormsAuthentication.SetAuthCookie(viewModel.Login, viewModel.LembrarMe);
                    string url = FormsAuthentication.GetRedirectUrl(viewModel.Login, viewModel.LembrarMe);
                    return new JavaScriptResult() { Script = $"document.location.href='{url}';" };
                }
                else
                {
                    ModelState.AddModelError("UsuarioOuSenhaInvalidos", "Usuário ou Senha inválidos");
                }
            }

            return PartialView("_errosValidacao", viewModel);
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
                ServicoAutenticacao servico = new ServicoAutenticacao(_contexto);
                var senha = servico.RecuperarSenha(viewModel.Email);
                if (senha != null)
                {
                    
                    ServicoNotificacao servicoNotificacao = new ServicoNotificacao(_contexto);
                    await Task.Run(() => servicoNotificacao.NotificarRecuperacaoSenha(viewModel.Email, senha, this.Url.Action("Login", "Autenticacao"))).ConfigureAwait(false);
                    ViewBag.Mensagem = "Enviamos um e-mail para você com uma senha temporária, utilize-a no seu próximo acesso.";
                }
                else
                {
                    ViewBag.Mensagem = "";
                    ModelState.AddModelError("EmailNaoEncontrado", "E-mail não encontrado");
                }
            }
            return View(viewModel);
        }


        public ActionResult Sair()
        {
            FormsAuthentication.SignOut();
            return Redirect("Login");
        }

    }
}
