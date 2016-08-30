using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartLMS.Dominio.Entidades;
using System.Data.Entity.Core.Objects;
using Carubbi.GenericRepository;

namespace SmartLMS.WebUI.Models
{
    public class UsuarioViewModel
    {
        public bool Ativo { get; private set; }
        public string Email { get; private set; }
        public Guid Id { get; private set; }
        public string Login { get; private set; }
        public string Nome { get; set; }
        public DateTime DataCriacao { get; set; }

        public string NomePerfil { get; set; }
        public string Senha { get; private set; }

        internal static UsuarioViewModel FromEntity(Usuario usuario)
        {
            return new UsuarioViewModel
            {
                Nome = usuario.Nome,
                NomePerfil = ObjectContext.GetObjectType(usuario.GetType()).Name,
             };
        }

        internal static UsuarioViewModel FromEntity(Aluno aluno)
        {
            return new UsuarioViewModel
            {
                Nome = aluno.Nome,
                NomePerfil = "Aluno",
                Email = aluno.Email,
                Login = aluno.Login,
                Senha = aluno.Senha,
                Ativo = aluno.Ativo,
                Id = aluno.Id,
                DataCriacao = aluno.DataCriacao
            };
        }

        internal static PagedListResult<UsuarioViewModel> FromEntityList(PagedListResult<Aluno> alunos)
        {
            PagedListResult<UsuarioViewModel> pagina = new PagedListResult<UsuarioViewModel>();

            pagina.HasNext = alunos.HasNext;
            pagina.HasPrevious = alunos.HasPrevious;
            pagina.Count = alunos.Count;
            List<UsuarioViewModel> viewModels = new List<UsuarioViewModel>();
            foreach (var item in alunos.Entities)
            {
                viewModels.Add(FromEntity(item));
            }

            pagina.Entities = viewModels;
            return pagina;
        }
    }
}
