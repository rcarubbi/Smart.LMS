using Carubbi.GenericRepository;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Entidades.Pessoa;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Objects;

namespace SmartLMS.WebUI.Models
{
    public class UsuarioViewModel
    {
        public bool Ativo { get;  set; }

        [Required(ErrorMessage = "O campo Email é obrigatório")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "O campo Email não é um email válido")]
        public string Email { get;  set; }
        public Guid Id { get;  set; }


        [Required(ErrorMessage = "O campo Email é obrigatório")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "O campo Login não é um email válido")]
        public string Login { get;  set; }

       

        [Required(ErrorMessage = "O campo Nome é obrigatório")]
        public string Nome { get; set; }

        public DateTime DataCriacao { get; set; }

        public string NomePerfil { get; set; }

        [Required(ErrorMessage = "O campo Senha é obrigatório")]
        [DataType(DataType.Password)]
        public string Senha { get;  set; }

        [Compare("Senha", ErrorMessage = "A confirmação de senha não confere")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "A confirmação de senha é obrigatória")]
        [Display(Name = "Confirmação de senha")]
        public string ConfirmarSenha { get;  set; }

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
                ConfirmarSenha = aluno.Senha,
                Ativo = aluno.Ativo,
                Id = aluno.Id,
                DataCriacao = aluno.DataCriacao
            };
        }


        internal static UsuarioViewModel FromEntity(Professor professor)
        {
            return new UsuarioViewModel
            {
                Nome = professor.Nome,
                NomePerfil = "Professor",
                Email = professor.Email,
                Login = professor.Login,
                Senha = professor.Senha,
                ConfirmarSenha = professor.Senha,
                Ativo = professor.Ativo,
                Id = professor.Id,
                DataCriacao = professor.DataCriacao
            };
        }

        internal static PagedListResult<UsuarioViewModel> FromEntityList(PagedListResult<Aluno> usuarios)
        {
            PagedListResult<UsuarioViewModel> pagina = new PagedListResult<UsuarioViewModel>();

            pagina.HasNext = usuarios.HasNext;
            pagina.HasPrevious = usuarios.HasPrevious;
            pagina.Count = usuarios.Count;
            List<UsuarioViewModel> viewModels = new List<UsuarioViewModel>();
            foreach (var item in usuarios.Entities)
            {
                viewModels.Add(FromEntity(item));
            }

            pagina.Entities = viewModels;
            return pagina;
        }


        internal static PagedListResult<UsuarioViewModel> FromEntityList(PagedListResult<Professor> usuarios)
        {
            PagedListResult<UsuarioViewModel> pagina = new PagedListResult<UsuarioViewModel>();

            pagina.HasNext = usuarios.HasNext;
            pagina.HasPrevious = usuarios.HasPrevious;
            pagina.Count = usuarios.Count;
            List<UsuarioViewModel> viewModels = new List<UsuarioViewModel>();
            foreach (var item in usuarios.Entities)
            {
                viewModels.Add(FromEntity(item));
            }

            pagina.Entities = viewModels;
            return pagina;
        }
    }
}
