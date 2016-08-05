using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SmartLMS.WebUI.Models
{
    public class AcessoAulaViewModel
    {
        public int Percentual { get; set; }

        public Guid IdAula { get; set; }


        public decimal Segundos { get; set; }
        public string NomeAula { get; private set; }
        public string NomeCurso { get; private set; }
        public string DataHoraTexto { get; private set; }

        internal AcessoAula ToEntity(Usuario usuario, Aula aula)
        {
            return new AcessoAula
            {
                DataHoraAcesso = DateTime.Now,
                Percentual = Percentual,
                Segundos = Segundos,
                Usuario = usuario,
                Aula = aula
            };
        }

        internal static IEnumerable<AcessoAulaViewModel> FromEntityList(IEnumerable<AcessoAula> aulas, DefaultDateTimeHumanizeStrategy humanizer)
        {

            foreach (var item in aulas)
            {
                yield return FromEntity(item, humanizer);
            }
           
        }

        private static AcessoAulaViewModel FromEntity(AcessoAula item, DefaultDateTimeHumanizeStrategy humanizer)
        {
            return new AcessoAulaViewModel
            {
                IdAula = item.Aula.Id,
                Percentual = item.Percentual,
                NomeAula = item.Aula.Nome,
                NomeCurso = item.Aula.Curso.Nome,
                DataHoraTexto = humanizer.Humanize(item.DataHoraAcesso, DateTime.Now, CultureInfo.CurrentUICulture),
            };
        }
    }
}
