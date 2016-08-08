using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartLMS.Dominio.Entidades;
using Humanizer.DateTimeHumanizeStrategy;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace SmartLMS.WebUI.Models
{
    public class ComentarioViewModel
    {
        public long Id { get; set; }


        public bool Editavel { get; set; }
        public Guid IdAula { get; set; }

        public DateTime DataHora { get; set; }



        public string DataHoraTexto { get; set; }

        public string NomeUsuario { get; set; }

        [Required(ErrorMessage = "Digite seu comentário")]
        public string Comentario { get; set; }

        internal static IEnumerable<ComentarioViewModel> FromEntityList(IEnumerable<Comentario> comentarios, DefaultDateTimeHumanizeStrategy humanizer, Guid idUsuarioLogado)
        {
            foreach (var item in comentarios)
            {
                yield return FromEntity(item, humanizer, idUsuarioLogado);

            }
        }

        private static ComentarioViewModel FromEntity(Comentario item, DefaultDateTimeHumanizeStrategy humanizer, Guid idUsuarioLogado)
        {
            return new ComentarioViewModel
            {
                Id = item.Id,
                DataHoraTexto = humanizer.Humanize(item.DataHora, DateTime.Now, CultureInfo.CurrentUICulture),
                Comentario = item.TextoComentario,
                NomeUsuario = item.Usuario.Nome,
                Editavel = item.Usuario.Id == idUsuarioLogado
            };
        }

        internal Comentario ToEntity(Usuario usuario, Aula aula)
        {
            return new Comentario
            {
                DataHora = this.DataHora,
                Aula = aula,
                Usuario = usuario,
                TextoComentario = this.Comentario
            };
        }
    }
}
