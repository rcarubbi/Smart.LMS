using Carubbi.GenericRepository;
using SmartLMS.Domain.Entities.Delivery;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SmartLMS.WebUI.Models
{
    public class ClassroomViewModel
    {
        [Required]

        public string Name { get; set; }

        [Required(ErrorMessage = "At least one course is required")]
        [Display(Name = "Courses")]
        public List<Guid> CourseIds { get; set; }

        [Display(Name = "Students")]
        public List<Guid> StudentIds { get; set; }

        public bool Active { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }


        internal static PagedListResult<ClassroomViewModel> FromEntityList(PagedListResult<Classroom> classrooms)
        {
            return new PagedListResult<ClassroomViewModel>
            {
                HasNext = classrooms.HasNext,
                HasPrevious = classrooms.HasPrevious,
                Count = classrooms.Count,
                Entities = classrooms.Entities.Select(FromEntity).ToList()
            };
        }

        internal static IEnumerable<ClassroomViewModel> FromEntityList(List<Classroom> classrooms)
        {
            foreach (var classroom in classrooms)
            {
                yield return FromEntity(classroom);
            }  
        }

        public static ClassroomViewModel FromEntity(Classroom item)
        {
            return new ClassroomViewModel
            {
                CreatedAt = item.CreatedAt,
                Active = item.Active,
                Id = item.Id,
                Name = item.Name,
                CourseIds = item.Courses.Select(a => a.CourseId).ToList(),
                StudentIds = item.DeliveryPlans.SelectMany(x => x.Students).OrderBy(a => a.Name).Select(x => x.Id).ToList()
            };
        }
    }
}