using Carubbi.Mailer.Implementation;
using NLog;
using Quartz;
using SmartLMS.Domain;
using SmartLMS.Domain.Repositories;
using System;

namespace SmartLMS.ContentDeliveryAgent
{
    public class ContentDeliveryJob : IJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IContext _context;
        public ContentDeliveryJob(IContext context)
        {
            _context = context;
        }

        public void Execute(IJobExecutionContext context)
        {
            Logger.Trace("Execution started");
            var classroomRepository = new ClassroomRepository(_context);
            var plans = classroomRepository.ListNotConcludedDeliveryPlans();
            foreach (var item in plans)
            {
                try
                {
                    Logger.Info($"Granting access according to the plan of the day {item.StartDate.ToShortDateString()} to the {item.Classroom.Name} classroom");
                    item.DeliverPendingClasses(_context, new SmtpSender());
                    Logger.Info($"Access granted");
                }
                catch (Exception ex)
                {
                   Logger.Error(ex, "Error on grant access");
                }
            }
            Logger.Trace("Execution ended");
        }

      }
}