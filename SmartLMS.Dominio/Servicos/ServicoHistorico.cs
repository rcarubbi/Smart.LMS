using Carubbi.GenericRepository;
using Carubbi.Utils.DataTypes;
using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Domain.Servicos;
using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Entidades.Historico;
using SmartLMS.Dominio.Entidades.Pessoa;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;

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
                        (!periodo.StartDate.HasValue || (periodo.StartDate.HasValue && periodo.StartDate.Value <= a.DataHoraAcesso)
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
            RepositorioTurma turmaRepo = new Repositorios.RepositorioTurma(_contexto);
            RepositorioUsuario usuarioRepo = new RepositorioUsuario(_contexto);

            var usuario = usuarioRepo.ObterPorId(idUsuario);
            var planejamentos = new List<long>();
            if (usuario is Aluno)
            {
                planejamentos = (usuario as Aluno).Planejamentos.Select(x => x.Id).ToList(); 
            }
            

            var avisos = _contexto.ObterLista<Aviso>()
             .Where(a =>
                DbFunctions.TruncateTime(a.DataHora) >= DbFunctions.TruncateTime(usuario.DataCriacao) && 
                 (!periodo.StartDate.HasValue || (periodo.StartDate.HasValue && periodo.StartDate.Value <= a.DataHora)
                 && (!periodo.EndDate.HasValue || (periodo.EndDate.HasValue && periodo.EndDate.Value >= a.DataHora)))
                   && (((tipo == TipoAviso.Pessoal || tipo == TipoAviso.Todos) && a.Usuario != null && a.Usuario.Id == idUsuario)
              || ((tipo == TipoAviso.Turma || tipo == TipoAviso.Todos) && a.Planejamento != null && planejamentos.Contains(a.Planejamento.Id))
              || ((tipo == TipoAviso.Geral || tipo == TipoAviso.Todos) && a.Planejamento == null && a.Usuario == null))
                 )
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
