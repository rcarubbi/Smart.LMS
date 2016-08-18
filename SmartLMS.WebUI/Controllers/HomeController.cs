using Carubbi.Mailer.Implementation;
using Carubbi.Utils.Data;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.Dominio.Servicos;
using SmartLMS.WebUI.Models;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        ServicoBuscaContextual servicoBusca;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            servicoBusca = new ServicoBuscaContextual(_contexto, _usuarioLogado);
        }

        public HomeController(IContexto contexto)
            : base(contexto)
        {

        }

        public ActionResult Index()
        {
            var areaRepo = new RepositorioAreaConhecimento(_contexto);
            var viewModel = AreaConhecimentoViewModel.FromEntityList(areaRepo.ListarAreasConhecimento(), 2);

            TempData["TituloAulasAssistidas"] = Parametro.TITULO_AULAS_ASSISTIDAS;
            TempData["TituloUltimasAulas"] = Parametro.TITULO_ULTIMAS_AULAS;
            return View(viewModel);
        }


        public ActionResult BuscaContextual(string term)
        {

            var resultados = servicoBusca.Pesquisar(term).Entities.Select(r => new { label = r.Descricao }).ToList();
            return Json(resultados, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BuscaContextualPaginada(string termo, int pagina)
        {
            var resultados = servicoBusca.Pesquisar(termo, pagina);
            return Json(new { paginaCorrente = pagina, resultados = resultados }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FaleConosco(FaleConoscoViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    SmtpSender sender = new SmtpSender();
                    sender.PortNumber = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_PORTA).Valor.To(0);
                    sender.Host = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_SERVIDOR).Valor;
                    sender.UseDefaultCredentials = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_USAR_CREDENCIAIS_PADRAO).Valor.To(false);
                    sender.UseSSL = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_USA_SSL).Valor.To(false);
                    if (!sender.UseDefaultCredentials)
                    {
                        sender.Username = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_USUARIO).Valor;
                        sender.Password = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.SMTP_SENHA).Valor;
                    }

                    var emailDestinatarioFaleConosco = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.CHAVE_EMAIL_DESTINATARIO_FALE_CONOSCO).Valor;
                    var nomeDestinatarioFaleConosco = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.CHAVE_NOME_DESTINATARIO_FALE_CONOSCO).Valor;
                    var emailRemetente = _contexto.ObterLista<Parametro>().Single(x => x.Chave == Parametro.REMETENTE_EMAIL).Valor;

                    MailMessage email = new MailMessage();
                    MailAddress destinatario = new MailAddress(emailDestinatarioFaleConosco, nomeDestinatarioFaleConosco);
                    email.To.Add(destinatario);
                    email.From = new MailAddress(emailRemetente, Parametro.PROJETO);
                    email.Body = $@"<div><h1>Fale Conosco - {Parametro.PROJETO}</h1>
                                    <dl>
                                        <dt>Nome:</dt>
                                        <dd>{viewModel.Nome}</dd>
                                        <dt>Email:</dt>
                                        <dd>{viewModel.Email}</dd>
                                        <dt>Mensagem:</dt>
                                        <dd>{viewModel.Mensagem}</dd>
                                    </dl>
                                    </div>";
                    email.Subject = $"Fale Conosco - {Parametro.PROJETO}";
                    sender.Send(email);
                    return Json(new { Success = true });
                }
                else
                {
                    return Json(new { Success = false });
                }
            }
            catch
            {

                return Json(new { Success = false });
            }
        }
    }
}