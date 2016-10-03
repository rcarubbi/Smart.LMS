using Carubbi.Utils.Data;
using Carubbi.Utils.DataTypes;
using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.Dominio.Servicos;
using SmartLMS.WebUI.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize]
    public class AvisoController : BaseController
    {
        public AvisoController(IContexto contexto)
            : base(contexto)
        {


        }


        [ChildActionOnly]
        public ActionResult ExibirAvisos()
        {
            var repo = new RepositorioAviso(_contexto);
            var avisos = repo.ListarAvisosNaoVistos(_usuarioLogado.Id);
            var dateTimeHumanizerStrategy = new DefaultDateTimeHumanizeStrategy();
            return PartialView("_PainelAvisos", AvisoViewModel.FromEntityList(avisos, dateTimeHumanizerStrategy));
        }

        [HttpPost]
        public ActionResult Visualizar(long idAviso, Guid idUsuario)
        {
            var repo = new RepositorioAviso(_contexto);
            var avisos = repo.ListarAvisosNaoVistos(idUsuario);

            var aviso = new UsuarioAviso
            {
                DataVisualizacao = DateTime.Now,
                IdAviso = idAviso,
                IdUsuario = idUsuario
            };
            _contexto.ObterLista<UsuarioAviso>().Add(aviso);
            _contexto.Salvar();
            var dateTimeHumanizerStrategy = new DefaultDateTimeHumanizeStrategy();
            var proximoAviso = repo.ListarAvisosNaoVistos(idUsuario).Except(avisos).FirstOrDefault();
            if (proximoAviso != null)
                return Json(AvisoViewModel.FromEntity(proximoAviso, dateTimeHumanizerStrategy));
            else
                return new EmptyResult();
        }

        public ActionResult Index()
        {
            TipoAviso tipo = TipoAviso.Geral;
            ViewBag.TiposAviso = new SelectList(tipo.ToDataSource<TipoAviso>(), "Key", "Value");
            var periodo = new DateRange();
            periodo.StartDate = DateTime.Now.AddMonths(-1);
            periodo.EndDate = DateTime.Now;
            ServicoHistorico servico = new ServicoHistorico(_contexto, new DefaultDateTimeHumanizeStrategy());
            return View(AvisoViewModel.FromEntityList(servico.ListarHistoricoAvisos(periodo, 1, _usuarioLogado.Id, TipoAviso.Todos)));

        }

        public ActionResult ListarHistoricoAvisos(DateTime? dataInicio, DateTime? dataFim, TipoAviso tipo = TipoAviso.Todos, int pagina = 1)
        {
            var periodo = new DateRange();
            periodo.StartDate = dataInicio;
            periodo.EndDate = dataFim;

            ServicoHistorico servico = new ServicoHistorico(_contexto, new DefaultDateTimeHumanizeStrategy());
            return Json(AvisoViewModel.FromEntityList(servico.ListarHistoricoAvisos(periodo, pagina, _usuarioLogado.Id, tipo)), JsonRequestBehavior.AllowGet);
        }
    }
}
