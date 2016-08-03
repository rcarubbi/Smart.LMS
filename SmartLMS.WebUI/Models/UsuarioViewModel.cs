using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartLMS.Dominio.Entidades;
using System.Data.Entity.Core.Objects;

namespace SmartLMS.WebUI.Models
{
    public class UsuarioViewModel
    {
        public string Nome { get; set; }

        public string NomePerfil { get; set; }

        internal static UsuarioViewModel FromEntity(Usuario usuario)
        {
            return new UsuarioViewModel
            {
                Nome = usuario.Nome,
                NomePerfil = ObjectContext.GetObjectType(usuario.GetType()).Name
        };
        }
    }
}
