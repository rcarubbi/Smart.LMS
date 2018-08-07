using Carubbi.Mailer.Interfaces;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SmartLMS.Domain.Entities.Delivery
{
    public class DeliveryPlan
    {
        
        public virtual ICollection<Notice> Notices { get; set; }

        public long Id { get; set; }

        public virtual ICollection<ClassDeliveryPlan> AvailableClasses { get; set; } = new List<ClassDeliveryPlan>();

        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual Classroom Classroom { get; set; }

        public DateTime StartDate { get; set; }

        public bool Concluded { get; set; }

        public void DeliverPendingClasses(IContext context, IMailSender sender)
        {
            if (Concluded)
                return;

            var classWasDelivered = false;
          
            do
            {
                var lastDeliveredClass = AvailableClasses.OrderByDescending(x => x.DeliveryDate).FirstOrDefault();
                var nextCourse = GetNextCourse(lastDeliveredClass);

                var nextClass = GetNextClass(lastDeliveredClass, lastDeliveredClass?.Class?.Course);
                if (nextClass == null && (lastDeliveredClass == null || (lastDeliveredClass.DeliveryDate.Date < DateTime.Today)))
                {
                    nextClass = GetNextClass(lastDeliveredClass, nextCourse);
                }

                if (nextClass == null)
                {
                    Concluded = nextCourse == null;
                    classWasDelivered = false;
                    context.Save();
                }
                else
                {
                    classWasDelivered = CheckDeliveryStatus(context, sender, nextClass);
                }
            } while (classWasDelivered);
        }

        internal void DeliverClass(IContext context, IMailSender sender, Class klass)
        {
            AvailableClasses.Add(new ClassDeliveryPlan
            {
                Class = klass,
                DeliveryDate = DateTime.Now,
                DeliveryPlan = this
            });

            context.GetList<Notice>().Add(new Notice
            {
                Text = $@"New <a href='Class/Watch/{klass.Id}'>{SmartLMS.Domain.Resources.Resource.ClassName} {klass.Name}</a> available! <br />
                               <a href='Class/Index/{klass.Course.Id}'>{SmartLMS.Domain.Resources.Resource.CourseName} {klass.Course.Name}</a> <br />",
                DateTime = DateTime.Now,
                DeliveryPlan = this,
            });
            context.Save();
            SendDeliveringClassEmail(context, sender, klass, Students);
        }

        private bool CheckDeliveryStatus(IContext contexto, IMailSender sender, Class klass)
        {
            DateTime? lastDeliveryDate = AvailableClasses.OrderByDescending(x => x.DeliveryDate).Select(x => x.DeliveryDate).FirstOrDefault();

            if ((lastDeliveryDate.Value).AddDays(klass.DeliveryDays).Date > DateTime.Today) return false;
            DeliverClass(contexto, sender, klass);
             
            return true;
        }

        public void SendDeliveringClassEmail(IContext context, IMailSender sender, Class klass, ICollection<Student> students)
        {
            var notificationService = new NotificationService(context, sender);
            foreach (var student in students)
            {
                notificationService.SendDeliveryClassEmail(klass, student);
            }
        }

   

        private Course GetNextCourse(ClassDeliveryPlan lastDeliveredClass)
        {
            var ordem = 0;
            if (lastDeliveredClass != null)
            {
                ordem = Classroom.Courses
                    .Single(x => x.CourseId == lastDeliveredClass.Class.Course.Id)
                    .Order;
            }

            var course = Classroom.Courses
                .Where(a => a.Course.Active)
                .OrderBy(x => x.Order)
                .FirstOrDefault(x => x.Order > ordem);

            return course?.Course;
        }

        private static Class GetNextClass(ClassDeliveryPlan lastDeliveredClass, Course course)
        {
            if (course == null)
                return null;
            var ordem = 0;
            if (lastDeliveredClass == null)
                return course.Classes
                    .Where(a => a.Active)
                    .OrderBy(x => x.Order)
                    .FirstOrDefault(x => x.Order > ordem);

            ordem = lastDeliveredClass.Class.Course.Id == course.Id ? lastDeliveredClass.Class.Order : 0;

            return course.Classes
                .Where(a => a.Active)
                .OrderBy(x => x.Order)
                .FirstOrDefault(x => x.Order > ordem);
        }

    }
}
