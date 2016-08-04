using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
    public class AvisoController : BaseController
    {
        public AvisoController(IContexto contexto)
            : base(contexto)
        {
           
             
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
