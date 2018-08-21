using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Carubbi.GenericRepository;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;

namespace SmartLMS.WebUI.Models
{
    public class CourseViewModel
    {
        [Required(ErrorMessage = "Select a teacher in charge of this course")]
        [Display(Name = "Teacher in charge")]
        public Guid TeacherInChargeId { get; set; }

        public string SubjectName { get; set; }

        [Required(ErrorMessage = "Select a subject")]
        [Display(Name = "Subject")]
        public Guid SubjectId { get; set; }

        [Required] public int Order { get; set; }

        [Required] public string Name { get; set; }

        public Guid Id { get; set; }

        public bool Active { get; set; }

        public string KnowledgeAreaName { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Image { get; set; }

        public string TeacherInChargeName { get; set; }

        public IEnumerable<ClassViewModel> Classes { get; set; }

        internal static IEnumerable<CourseViewModel> FromEntityList(IEnumerable<Course> courses, int depth)
        {
            return courses.Select(item => FromEntity(item, depth));
        }

        internal static PagedListResult<CourseViewModel> FromEntityList(PagedListResult<Course> courses)
        {
            return new PagedListResult<CourseViewModel>
            {
                HasNext = courses.HasNext,
                HasPrevious = courses.HasPrevious,
                Count = courses.Count,
                Entities = courses.Entities.Select(item => FromEntity(item, 0)).ToList()
            };
        }

        public static CourseViewModel FromEntity(Course item, int depth)
        {
            return new CourseViewModel
            {
                Active = item.Active,
                CreatedAt = item.CreatedAt,
                SubjectId = item.Subject.Id,
                SubjectName = item.Subject.Name,
                KnowledgeAreaName = item.Subject.KnowledgeArea.Name,
                Image = item.Image,
                Order = item.Order,
                Name = item.Name,
                Id = item.Id,
                TeacherInChargeId = item.TeacherInCharge.Id,
                TeacherInChargeName = item.TeacherInCharge.Name,
                Classes = depth > 2
                    ? ClassViewModel.FromEntityList(item.Classes.Where(a => a.Active).OrderBy(x => x.Order), depth)
                    : new List<ClassViewModel>()
            };
        }

        internal static IEnumerable<CourseViewModel> FromEntityList(List<ClassroomCourse> classroomCourses)
        {
            return classroomCourses.OrderBy(x => x.Order).Select(FromEntity);
        }

        private static CourseViewModel FromEntity(ClassroomCourse item)
        {
            return new CourseViewModel
            {
                Name = item.Course.Name,
                Id = item.Course.Id,
                Order = item.Order
            };
        }

        internal static CourseViewModel FromEntity(CourseIndex index)
        {
            return new CourseViewModel
            {
                SubjectId = index.Course.Subject.Id,
                Image = index.Course.Image,
                Order = index.Course.Order,
                Name = index.Course.Name,
                Id = index.Course.Id,
                TeacherInChargeId = index.Course.TeacherInCharge.Id,
                TeacherInChargeName = index.Course.TeacherInCharge.Name,
                Classes = ClassViewModel.FromEntityList(index.ClassesInfo)
            };
        }

        internal static Course ToEntity(CourseViewModel course, Subject subject, Teacher teacher)
        {
            return new Course
            {
                Id = course.Id,
                Name = course.Name,
                CreatedAt = course.CreatedAt,
                Subject = subject,
                Active = course.Active,
                Order = course.Order,
                TeacherInCharge = teacher,
                Image = course.Image
            };
        }
    }
}