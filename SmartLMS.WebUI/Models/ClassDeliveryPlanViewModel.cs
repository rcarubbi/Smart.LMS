using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.Delivery;

namespace SmartLMS.WebUI.Models
{
    public class ClassDeliveryPlanViewModel
    {

        public Guid ClassId { get; set; }

        public string ClassName { get; set; }

        public string CourseName { get; set; }

        public int ClassOrder { get; set; }

        public int CourseOrder { get; set; }

        public bool Available { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public int DaysToDeliver { get; set; }


        public static IList<ClassDeliveryPlanViewModel> FromEntityList(List<ClassDeliveryPlan> entities)
        {
            return entities.ConvertAll(FromEntity);
        }

        private static ClassDeliveryPlanViewModel FromEntity(ClassDeliveryPlan input)
        {
            return new ClassDeliveryPlanViewModel
            {
                DeliveryDate = input.DeliveryDate,
                DaysToDeliver = input.Class.DeliveryDays,
                Available =  input.DeliveryPlan.AvailableClasses.Any(x => x.ClassId == input.Class.Id),
                CourseName = input.Class.Course.Name,
                CourseOrder =  input.DeliveryPlan.Classroom.Courses.Single(c => c.CourseId == input.Class.Course.Id).Order,
                ClassId = input.Class.Id,
                ClassName =  input.Class.Name,
                ClassOrder =  input.Class.Order
            };
        }
    }
}