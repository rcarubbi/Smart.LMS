using Carubbi.GenericRepository;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Objects;
using System.Linq;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.WebUI.Models
{
    public class UserViewModel
    {
        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Required(ErrorMessage = "Email required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; }
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Login required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid login")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Select a classroom")]
        [Display(Name = "Classroom")]

        public Guid ClassroomId { get; set; }

        [Required(ErrorMessage = "Name required")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public string RoleName { get; set; }

        [Required(ErrorMessage = "Password required")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Password doesn't match")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password confirmation required")]
        [Display(Name = "Password confirmation")]
        public string ConfirmPassword { get; set; }

        internal static UserViewModel FromEntity(User user)
        {
            return new UserViewModel
            {
                Name = user.Name,
                RoleName = ObjectContext.GetObjectType(user.GetType()).Name,
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
