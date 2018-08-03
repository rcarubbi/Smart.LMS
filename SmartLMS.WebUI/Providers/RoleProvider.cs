using Carubbi.Utils.Data;
using SmartLMS.DAL;
using System;
using System.Linq;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;

namespace SmartLMS.WebUI.Providers
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
            using (var context = new Context())
            {
                var userRepository = new UserRepository(context);
                var users = userRepository.ListByRole((Role)Enum.Parse(typeof(Role), roleName));
                return users.Where(x => x.Login.Contains(usernameToMatch))
                    .Select(x => x.Login)
                    .ToArray();
            }
        }

        public override string[] GetAllRoles()
        {
            const Role roleEnum = Role.Admin;
            var roles = roleEnum.ToDataSource<Role>().Select(x => x.Value).ToArray();
            return roles;
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var context = new Context())
            {
                var userRepository = new UserRepository(context);
                return new[] { context.UnProxy(userRepository.GetByLogin(username)).GetType().Name };
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            using (var context = new Context())
            {
                var userRepository = new UserRepository(context);
                return userRepository.ListByRole((Role)Enum.Parse(typeof(Role), roleName)).Select(x => x.Login).ToArray();
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            using (var context = new Context())
            {
                var userRepository = new UserRepository(context);
                return (context.UnProxy(userRepository.GetByLogin(username)).GetType().Name == roleName);
            }
        }


        public override bool RoleExists(string roleName)
        {
            var roles = GetAllRoles();
            return roles.Any(x => x == roleName);
        }
    }
}
