using System;
using System.Collections.Generic;
using SmartLMS.Dominio.Entidades;
using Humanizer.DateTimeHumanizeStrategy;
using System.Globalization;
using Carubbi.GenericRepository;
using SmartLMS.Dominio.Servicos;
using SmartLMS.Domain.Servicos;

namespace SmartLMS.WebUI.Models
{
    public class AvisoViewModel
    {
        public long Id { get; set; }
        public bool MensagemDireta { get; set; }

        public string DataHora { get; set; }

        public DateTime? DataTurma { get; set; }

        public string DataTurmaTexto { get {
                return DataTurma.HasValue? DataTurma.Value.ToShortDateString() : string.Empty;
            }
        }

        public TipoAviso Tipo
        {
            get
            {
                if (MensagemDireta)
                    return TipoAviso.Pessoal;
                else if (DataTurma.HasValue)
                    return TipoAviso.Turma;
                else
                    return TipoAviso.Geral;
            }
        }
        public string Texto { get; set; }

        internal static IEnumerable<AvisoViewModel> FromEntityList(IEnumerable<Aviso> avisos, DefaultDateTimeHumanizeStrategy humanizer)
        {
            foreach (var item in avisos)
            {
                yield return FromEntity(item, humanizer);
            }
        }

        internal static AvisoViewModel FromEntity(Aviso item, DefaultDateTimeHumanizeStrategy humanizer)
        {
            return new AvisoViewModel
            {
                Id = item.Id,
                DataHora = humanizer.Humanize(item.DataHora, DateTime.Now, CultureInfo.CurrentUICulture),
                Texto = item.Texto,
                DataTurma = item.Turma != null ? item.Turma.DataInicio : (DateTime?)null,
                MensagemDireta = item.Usuario != null,
            };
        }

        public static PagedListResult<AvisoViewModel> FromEntityList(PagedListResult<AvisoInfo> avisos)
        {

            PagedListResult<AvisoViewModel> pagina = new PagedListResult<AvisoViewModel>();

            pagina.HasNext = avisos.HasNext;
            pagina.HasPrevious = avisos.HasPrevious;
            pagina.Count = avisos.Count;
            List<AvisoViewModel> viewModels = new List<AvisoViewModel>();
            foreach (var item in avisos.Entities)
            {
                viewModels.Add(FromEntity(item));
            }

            pagina.Entities = viewModels;
            return pagina;
        }

        private static AvisoViewModel FromEntity(AvisoInfo item)
        {
            return new AvisoViewModel
            {
                DataHora = item.DataHoraTexto,
                DataTurma = item.Aviso.Turma != null? item.Aviso.Turma.DataInicio : (DateTime?)null,
                Texto = item.Aviso.Texto,
                MensagemDireta = item.Aviso.Usuario != null
            };
        }

        public string TipoDescricao
        {
            get {
                return Tipo.ToString();
            }
        }
    }
}
