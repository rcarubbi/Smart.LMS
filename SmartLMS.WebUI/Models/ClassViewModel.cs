using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Carubbi.GenericRepository;
using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Domain.Attributes;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;
using SmartLMS.Domain.Resources;

namespace SmartLMS.WebUI.Models
{
    public class ClassViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "SelectTeacher")]
        [LocalizedDisplay("TeacherName")]
        public Guid TeacherId { get; set; }

        [Required] public int Order { get; set; }

        [LocalizedDisplay("DaysToDeliverLabel")]
        [Required]
        public int DeliveryDays { get; set; }


        public bool Available { get; set; }

        public string CreatedAtDescription { get; set; }


        public string CourseName { get; set; }


        public decimal Percentual { get; set; }

        public decimal WatchedSeconds { get; set; }

        [LocalizedDisplay("CourseName")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "SelectCourse")]
        public Guid CourseId { get; set; }

        public Guid Id { get; set; }


        [LocalizedDisplay("FileName")] public IEnumerable<FileViewModel> Files { get; set; }

        [Required] public string Content { get; set; }

        [Required] public string Name { get; set; }

        public string TeacherName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "SelectContentType")]
        [LocalizedDisplay("ContentTypeName")]
        public ContentType ContentType { get; set; }

        public DateTime DeliveryDate { get; set; }

        public string DeliveryDateDescription { get; set; }

        public string SubjectName { get; set; }
        public string KnowledgeAreaName { get; set; }

        public DateTime CreatedAt { get; set; }

        [LocalizedDisplay("ActiveFieldName")] public bool Active { get; set; }

        public bool Editable { get; set; }

        internal static IEnumerable<ClassViewModel> FromEntityList(IEnumerable<Class> classes, int depth,
            DefaultDateTimeHumanizeStrategy humanizer)
        {
            return classes.Select(item => FromEntity(item, depth, humanizer));
        }

        internal static PagedListResult<ClassViewModel> FromEntityList(PagedListResult<Class> classes)
        {
            return new PagedListResult<ClassViewModel>
            {
                HasNext = classes.HasNext,
                HasPrevious = classes.HasPrevious,
                Count = classes.Count,
                Entities = classes.Entities.Select(FromEntity).ToList()
            };
        }

        private static ClassViewModel FromEntity(Class item)
        {
            return new ClassViewModel
            {
                Id = item.Id,
                CourseId = item.Course.Id,
                Name = item.Name,
                Content = item.Content,
                ContentType = item.ContentType,
                TeacherName = item.Teacher.Name,
                CourseName = item.Course.Name,
                SubjectName = item.Course.Subject.Name,
                KnowledgeAreaName = item.Course.Subject.KnowledgeArea.Name,
                CreatedAt = item.CreatedAt,
                Active = item.Active,
                Order = item.Order,
                DeliveryDays = item.DeliveryDays
            };
        }

        public static ClassViewModel FromEntity(Class item, int depth, DefaultDateTimeHumanizeStrategy humanizer)
        {
            return new ClassViewModel
            {
                Id = item.Id,
                TeacherId = item.Teacher.Id,
                TeacherName = item.Teacher.Name,
                CourseId = item.Course.Id,
                CourseName = item.Course.Name,
                Name = item.Name,
                Content = item.Content,
                ContentType = item.ContentType,
                CreatedAt = item.CreatedAt,
                DeliveryDays = item.DeliveryDays,
                CreatedAtDescription = humanizer != null
                    ? humanizer.Humanize(item.CreatedAt, DateTime.Now, CultureInfo.CurrentUICulture)
                    : string.Empty,
                Active = item.Active,
                Order = item.Order,
                Files = depth > 3 ? FileViewModel.FromEntityList(item.Files) : new List<FileViewModel>()
            };
        }

        internal static IEnumerable<ClassViewModel> FromEntityList(IOrderedEnumerable<Class> classes, int depth)
        {
            return FromEntityList(classes, depth, null);
        }

        internal static IEnumerable<ClassViewModel> FromEntityList(IEnumerable<ClassInfo> classInfos)
        {
            return classInfos.Select(FromEntity);
        }

        public static ClassViewModel FromEntity(ClassInfo item)
        {
            return new ClassViewModel
            {
                Id = item.Class.Id,
                CourseId = item.Class.Course.Id,
                Name = item.Class.Name,
                Content = item.Class.Content,
                ContentType = item.Class.ContentType,
                TeacherName = item.Class.Teacher.Name,
                CreatedAt = item.Class.CreatedAt,
                CourseName = item.Class.Course.Name,
                Available = item.Available,
                Percentual = item.Percentual,
                WatchedSeconds = item.WatchedSeconds,
                Editable = item.Editable
            };
        }

        public static ClassViewModel FromEntityWithFiles(ClassInfo item)
        {
            return new ClassViewModel
            {
                Id = item.Class.Id,
                CourseId = item.Class.Course.Id,
                Name = item.Class.Name,
                Content = item.Class.Content,
                ContentType = item.Class.ContentType,
                TeacherName = item.Class.Teacher.Name,
                CreatedAt = item.Class.CreatedAt,
                CourseName = item.Class.Course.Name,
                Available = item.Available,
                Percentual = item.Percentual,
                WatchedSeconds = item.WatchedSeconds,
                Editable = item.Editable,
                Files = FileViewModel.FromEntityList(item.Class.Files.Where(x => x.Active))
            };
        }

        internal static IEnumerable<ClassViewModel> FromEntityList(IEnumerable<ClassDeliveryPlan> classes,
            DefaultDateTimeHumanizeStrategy humanizer)
        {
            return classes.Select(item => FromEntity(item, humanizer));
        }

        private static ClassViewModel FromEntity(ClassDeliveryPlan item, IDateTimeHumanizeStrategy humanizer)
        {
            return new ClassViewModel
            {
                Id = item.Class.Id,
                CourseId = item.Class.Course.Id,
                Name = item.Class.Name,
                Content = item.Class.Content,
                ContentType = item.Class.ContentType,
                TeacherName = item.Class.Teacher.Name,
                CreatedAt = item.Class.CreatedAt,
                DeliveryDate = item.DeliveryDate,
                CourseName = item.Class.Course.Name,
                CreatedAtDescription = humanizer != null
                    ? humanizer.Humanize(item.Class.CreatedAt, DateTime.Now, CultureInfo.CurrentUICulture)
                    : string.Empty,
                DeliveryDateDescription = humanizer != null
                    ? humanizer.Humanize(item.DeliveryDate, DateTime.Now, CultureInfo.CurrentUICulture)
                    : string.Empty,
                Available = true
            };
        }

        internal static Class ToEntity(ClassViewModel klass, Course course, Teacher teacher)
        {
            return new Class
            {
                Id = klass.Id,
                CreatedAt = klass.CreatedAt,
                Active = klass.Active,
                DeliveryDays = klass.DeliveryDays,
                Content = klass.Content,
                Name = klass.Name,
                Order = klass.Order,
                ContentType = klass.ContentType,
                Teacher = teacher,
                Course = course
            };
        }
    }
}