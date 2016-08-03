using Carubbi.Utils.Data;
using SmartLMS.DAL;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Linq;
namespace SmartLMS.WebUI.Servicos
{
    public class RoleProvider : System.Web.Security.RoleProvider
    {
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotSupportedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotSupportedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotSupportedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotSupportedException();
        }

        public override string ApplicationName
        {
            get;
            set;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (var contexto = new Contexto())
            {
                RepositorioUsuario usuarioRepo = new RepositorioUsuario(contexto);
                var usuarios = usuarioRepo.ListarPorPerfil((Perfil)Enum.Parse(typeof(Perfil), roleName));
                return usuarios.Where(x => x.Login.Contains(usernameToMatch))
                    .Select(x => x.Login)
                    .ToArray();
            }
        }

        public override string[] GetAllRoles()
        {
            Perfil perfilEnum = Perfil.Administrador;
            string[] perfis = perfilEnum.ToDataSource<Perfil>().Select(x => x.Value).ToArray();
            return perfis;
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var contexto = new Contexto())
            {
                RepositorioUsuario usuarioRepo = new RepositorioUsuario(contexto);
                return new string[] { usuarioRepo.ObterPorLogin(username).GetType().Name };
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            using (var contexto = new Contexto())
            {
                RepositorioUsuario usuarioRepo = new RepositorioUsuario(contexto);
                return usuarioRepo.ListarPorPerfil((Perfil)Enum.Parse(typeof(Perfil), roleName)).Select(x => x.Login).ToArray();
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            using (var contexto = new Contexto())
            {
                RepositorioUsuario usuarioRepo = new RepositorioUsuario(contexto);
                return (usuarioRepo.ObterPorLogin(username).GetType().Name == roleName);
            }
        }


        public override bool RoleExists(string roleName)
        {
            string[] perfis = GetAllRoles();
            return perfis.Any(x => x == roleName);
        }
    }
}
