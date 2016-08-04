using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
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
            var dateTimeHumanizerStrategy = new DefaultDateTimeHumanizeStrategy();
            return PartialView("_PainelAvisos", AvisoViewModel.FromEntityList(repo.ListarAvisosNaoVistos(_usuarioLogado.Id), dateTimeHumanizerStrategy));
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
    }
}
