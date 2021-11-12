using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Carubbi.GenericRepository;
using Carubbi.Utils.Localization;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Resources;

namespace SmartLMS.WebUI.Models
{
    public class UserViewModel
    {
        [LocalizedDisplay("ActiveFieldName", typeof(Resource))] public bool Active { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailRequired")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; }

        public Guid Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "LoginRequired")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "InvalidLogin")]
        public string Login { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ClassroomRequired")]
        [LocalizedDisplay("ClassroomName", typeof(Resource))]
        public Guid ClassroomId { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "UserNameRequired")]
        [LocalizedDisplay("UserNameFieldName", typeof(Resource))]
        public string Name { get; set; }


        public DateTime CreatedAt { get; set; }

        public string RoleName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "UserPasswordRequired")]
        [LocalizedDisplay("PasswordFieldName", typeof(Resource))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessageResourceType = typeof(Resource),
            ErrorMessageResourceName = "PasswordDoenstMatch")]
        [DataType(DataType.Password)]
        [Required(ErrorMessageResourceType = typeof(Resource),
            ErrorMessageResourceName = "PasswordConfirmationRequired")]
        [LocalizedDisplay("PasswordConfirmationFieldName", typeof(Resource))]
        public string ConfirmPassword { get; set; }

        internal static UserViewModel FromEntity(User user)
        {
            return new UserViewModel
            {
                Name = user.Name,
                RoleName = ObjectContext.GetObjectType(user.GetType()).Name
            };
        }

        internal static UserViewModel FromEntity(Student student)
        {
            return new UserViewModel
            {
                Name = student.Name,
                RoleName = "Student",
                Email = student.Email,
                Login = student.Login,
                Password = student.Password,
                ConfirmPassword = student.Password,
                Active = student.Active,
                Id = student.Id,
                CreatedAt = student.CreatedAt
            };
        }

        internal static UserViewModel FromEntity(Teacher teacher)
        {
            return new UserViewModel
            {
                Name = teacher.Name,
                RoleName = "Teacher",
                Email = teacher.Email,
                Login = teacher.Login,
                Password = teacher.Password,
                ConfirmPassword = teacher.Password,
                Active = teacher.Active,
                Id = teacher.Id,
                CreatedAt = teacher.CreatedAt
            };
        }

        internal static PagedListResult<UserViewModel> FromEntityList(PagedListResult<Student> students)
        {
            return new PagedListResult<UserViewModel>
            {
                HasNext = students.HasNext,
                HasPrevious = students.HasPrevious,
                Count = students.Count,
                Entities = students.Entities.Select(FromEntity).ToList()
            };
        }

        internal static PagedListResult<UserViewModel> FromEntityList(PagedListResult<Teacher> teachers)
        {
            return new PagedListResult<UserViewModel>
            {
                HasNext = teachers.HasNext,
                HasPrevious = teachers.HasPrevious,
                Count = teachers.Count,
                Entities = teachers.Entities.Select(FromEntity).ToList()
            };
        }
    }
}