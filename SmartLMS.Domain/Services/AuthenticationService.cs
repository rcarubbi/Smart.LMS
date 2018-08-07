using Carubbi.DiffAnalyzer;
using Carubbi.Mailer.Interfaces;
using Carubbi.Utils.Security;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;
using System;
using System.Linq;
using System.Text;
using SmartLMS.Domain.Resources;

namespace SmartLMS.Domain.Services
{
    public class AuthenticationService
    {
        private readonly IContext _context;
        private readonly CriptografiaSimetrica _crypt;
        private readonly NotificationService _notificationService;

        public AuthenticationService(IContext context, IMailSender sender)
        {
            _context = context;
            _notificationService = new NotificationService(_context, sender);
            _crypt = new CriptografiaSimetrica(SymmetricCryptProvider.TripleDES);
            var parameterRepository = new ParameterRepository(_context);
            _crypt.Key = parameterRepository.GetValueByKey(Parameter.CRYPTO_KEY);
        }


        public bool Login(string login, string password)
        {
            var encryptedPassword = _crypt.Encrypt(password);
            return _context.GetList<User>().Any(u => u.Login == login && u.Password == encryptedPassword && u.Active);
        }

        public void UpdatedUser(Guid id, string name, string email, string login, string password, bool active, Role role, User loggedUser)
        {
            var userRepository = new UserRepository(_context);
            var user = userRepository.GetById(id);
            var userByAccount = userRepository.GetByLogin(login);

            if (userByAccount != null && userByAccount.Id != id)
            {
                throw new ApplicationException(Resource.AnotherUserToThisLoginException);
            }

            User updatedUser = null;

            switch (role)
            {
                case Role.Admin:
                    updatedUser = new Admin();
                    break;
                case Role.Teacher:
                    updatedUser = new Teacher();
                    break;
                case Role.Student:
                    updatedUser = new Student();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }


            updatedUser.Id = id;
            updatedUser.Name = name;
            updatedUser.Active = active;
            updatedUser.Password = password != user.Password ? _crypt.Encrypt(password) : password;

            updatedUser.Login = login;
            updatedUser.Email = email;
            updatedUser.CreatedAt = user.CreatedAt;
            

            var analyzer = new DiffAnalyzer(1);
            var differences = analyzer.Compare(_context.UnProxy(user), updatedUser, a => a.State == DiffState.Modified);

            
            var diffText = new StringBuilder($"{Resource.PersonalInfoUpdatedTitle}:{Environment.NewLine}<br />");
            foreach (var item in differences)
            {
                diffText.AppendLine(item.PropertyName == "Password"
                    ? $"- {Resource.PasswordUpdatedNoticeMessage} <br />"
                    : $"- {item.PropertyName} {Resource.PersonalInfoUpdatedFrom} {item.OldValue} {Resource.PersonalInfoUpdatedTo} {item.NewValue}<br />");
            }

            var updatingNotice = new Notice {
                Text = diffText.ToString(),
                DateTime = DateTime.Now,
                User = user,
            };

            _context.GetList<Notice>().Add(updatingNotice);
            _context.Update(user, updatedUser);
            _context.Save(loggedUser);
 
        }

        public User CreateUser(string name, string login, string email, string password, Role role, User loggedUser)
        {
            var userRepository = new UserRepository(_context);

            if (userRepository.GetByLogin(login) != null)
                throw new ApplicationException(Resource.AnotherUserToThisLoginException);

            var encryptedPassword = _crypt.Encrypt(password);
            User user = null;

            switch (role)
            {
                case Role.Admin:
                    user = new Admin();
                    break;
                case Role.Teacher:
                    user = new Teacher();
                    break;
                case Role.Student:
                    user = new Student();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }

            user.Login = login;
            user.Password = encryptedPassword;
            user.Active = true;
            user.Email = email;
            user.Name = name;
            user.CreatedAt = DateTime.Now;

            if (role == Role.Student)
            {
                var notice = new Notice
                {
                    User = user,
                    Text = Resource.WelcomeNoticeMessage,
                    DateTime = DateTime.Now,
                };
                _context.GetList<Notice>().Add(notice);
            }

            userRepository.Save(user);
            _context.Save(loggedUser);

            _notificationService.SendCreatingUserNotiication(user, password);
            return user;
        }

        public string RecoverPassword(string email)
        {
            var userRepository = new UserRepository(_context);
            var user = userRepository.GetByEmail(email);
            return user == null ? null : _crypt.Decrypt(user.Password);

        }
    }
}
