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
using SmartLMS.Domain.Servicos;

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

        public PagedListResult<AvisoInfo> ListarHistoricoAvisos(DateRange periodo, int pagina, Guid idUsuario, TipoAviso tipo)
        {
            Repositorios.RepositorioTurma turmaRepo = new Repositorios.RepositorioTurma(_contexto);
            var turmas = turmaRepo.ListarTurmasPorAluno(idUsuario);



            var avisos = _contexto.ObterLista<Aviso>()
             .Where(a =>
                 (!periodo.StartDate.HasValue || (periodo.StartDate.HasValue && periodo.StartDate.Value <= a.DataHora)
                 && (!periodo.EndDate.HasValue || (periodo.EndDate.HasValue && periodo.EndDate.Value >= a.DataHora)))
                 && (((tipo == TipoAviso.Pessoal || tipo == TipoAviso.Todos) && a.Usuario != null && a.Usuario.Id == idUsuario)
                 || ((tipo == TipoAviso.Turma || tipo == TipoAviso.Todos) && a.Turma != null && tipo == TipoAviso.Turma && turmas.Any(t => t.Id == a.Turma.Id))
                 || ((tipo == TipoAviso.Geral || tipo == TipoAviso.Todos) && a.Turma == null && a.Usuario == null)))
                 .Select(x => new AvisoInfo { Aviso = x });


            GenericRepository<AvisoInfo> repo = new GenericRepository<AvisoInfo>(_contexto, avisos);
            var query = new SearchQuery<AvisoInfo>() { Take = 8, Skip = (pagina - 1) * 8 };
            query.SortCriterias.Add(new DynamicFieldSortCriteria<AvisoInfo>("Aviso.DataHora desc"));

            var paginaResultados = repo.Search(query);

            foreach (var item in paginaResultados.Entities)
            {
                item.DataHoraTexto = _humanizer.Humanize(item.Aviso.DataHora, DateTime.Now, CultureInfo.CurrentUICulture);
            }

            return paginaResultados;
        }
    }
}
