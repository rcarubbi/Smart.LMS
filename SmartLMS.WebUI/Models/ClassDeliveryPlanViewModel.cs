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

        public static ClassDeliveryPlanViewModel FromEntity(ClassDeliveryPlan entity)
        {
            return new ClassDeliveryPlanViewModel
            {
                ClassId =  entity.ClassId,
                CourseName = entity.Class.Course.Name,
                ClassName = entity.Class.Name,
                ClassOrder = entity.Class.Order,
                CourseOrder = entity.DeliveryPlan.Classroom.Courses.Single(x => x.CourseId == entity.Class.Course.Id).Order,
                Available = true,
                DeliveryDate = entity.DeliveryDate,
                DaysToDeliver =  entity.Class.DeliveryDays
            };
        }

        public static ClassDeliveryPlanViewModel FromEntity(Class entity, DeliveryPlan deliveryPlan)
        {
            return new ClassDeliveryPlanViewModel
            {
                ClassId = entity.Id,
                CourseName = entity.Course.Name,
                ClassName = entity.Name,
                ClassOrder = entity.Order,
                CourseOrder = deliveryPlan.Classroom.Courses.Single(x => x.CourseId == entity.Course.Id).Order,
                Available = false,
                DaysToDeliver = entity.DeliveryDays
            };
        }


        internal static IList<ClassDeliveryPlanViewModel> FromClassEntityList(List<Class> classes, DeliveryPlan deliveryPlan)
        {
            return classes.ConvertAll(klass => FromEntity(klass, deliveryPlan));
        }
    }
}