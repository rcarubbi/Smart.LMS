using System;
using System.Collections.Generic;
using SmartLMS.Dominio.Entidades;
using Humanizer.DateTimeHumanizeStrategy;
using System.Globalization;
using Carubbi.GenericRepository;
using SmartLMS.Dominio.Servicos;
using SmartLMS.Domain.Servicos;
using SmartLMS.Dominio.Entidades.Comunicacao;

namespace SmartLMS.WebUI.Models
{
    public class AvisoViewModel
    {
        public long Id { get; set; }
        public bool MensagemDireta { get; set; }

        public string DataHora { get; set; }

        public string NomeTurma { get; set; }

        
        public TipoAviso Tipo
        {
            get
            {
                if (MensagemDireta)
                    return TipoAviso.Pessoal;
                else if (!string.IsNullOrEmpty(NomeTurma))
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
                NomeTurma = item.Planejamento?.Turma?.Nome,
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
                NomeTurma = item.Aviso?.Planejamento?.Turma?.Nome,
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
