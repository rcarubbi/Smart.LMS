using Carubbi.GenericRepository;
using Carubbi.Utils.DataTypes;
using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Dominio.Servicos
{
    public class ServicoHistorico
    {
        private DefaultDateTimeHumanizeStrategy _humanizer;
        private IContexto _contexto;
        public ServicoHistorico(IContexto contexto, DefaultDateTimeHumanizeStrategy humanizer)
        {
            _contexto = contexto;
            _humanizer = humanizer;
        }
        public PagedListResult<AcessoInfo> ListarHistorico(DateRange periodo, int pagina, Guid idUsuario, TipoAcesso tipo)
        {
            var acessos = _contexto.ObterLista<AcessoAula>()
                .Where(a =>
                    (!periodo.StartDate.HasValue || (periodo.StartDate.HasValue && periodo.StartDate.Value <= a.DataHoraAcesso)
                    && (!periodo.EndDate.HasValue || (periodo.EndDate.HasValue && periodo.EndDate.Value >= a.DataHoraAcesso)
                    && a.Usuario.Id == idUsuario))
                    && (tipo == TipoAcesso.Aula || tipo == TipoAcesso.Todos))
                .Select(a =>
                    new AcessoInfo
                    {
                        AcessoArquivo = null,
                        AcessoAula = a,
                        Tipo = TipoAcesso.Aula,
                        DataHoraAcesso = a.DataHoraAcesso
                    })
                .Union(
                  _contexto.ObterLista<AcessoArquivo>()
                  .Where(a =>
                        (!periodo.StartDate.HasValue || (periodo.StartDate.HasValue && periodo.StartDate.Value >= a.DataHoraAcesso)
                        && (!periodo.EndDate.HasValue || (periodo.EndDate.HasValue && periodo.EndDate.Value >= a.DataHoraAcesso)
                        && a.Usuario.Id == idUsuario))
                        && (tipo == TipoAcesso.Arquivo || tipo == TipoAcesso.Todos))
                        .Select(a =>
                            new AcessoInfo
                            {
                                AcessoArquivo = a,
                                AcessoAula = null,
                                Tipo = TipoAcesso.Arquivo,
                                DataHoraAcesso = a.DataHoraAcesso
                            }).OrderByDescending(a => a.DataHoraAcesso)
                )
                .OrderByDescending(x => x.DataHoraAcesso);

            GenericRepository<AcessoInfo> repo = new GenericRepository<AcessoInfo>(_contexto, acessos);
            var query = new SearchQuery<AcessoInfo>() { Take = 8, Skip = (pagina - 1) * 8 };
            query.SortCriterias.Add(new Carubbi.GenericRepository.DynamicFieldSortCriteria<AcessoInfo>("DataHoraAcesso desc"));

            var paginaResultados = repo.Search(query);
            
            foreach (var item in paginaResultados.Entities)
            {
                item.DataHoraTexto = _humanizer.Humanize(item.DataHoraAcesso, DateTime.Now, CultureInfo.CurrentUICulture);
            }

            return paginaResultados;
        }
    }
}
