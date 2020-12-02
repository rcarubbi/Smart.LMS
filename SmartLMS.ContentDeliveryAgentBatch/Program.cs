using Carubbi.Mailer.Implementation;
using NLog;
using SmartLMS.DAL;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Repositories;
using System;
using System.Configuration;
using System.Globalization;
using System.Threading;

namespace SmartLMS.ContentDeliveryAgentBatch
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo culture = new CultureInfo(ConfigurationManager.AppSettings["DefaultCulture"]);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            Logger Logger = LogManager.GetCurrentClassLogger();
            IContext _context = new Context();
            Parameter.LoadParameters(_context);
            Console.WriteLine($"{Parameter.APP_NAME} - Content access agent started");
            Logger.Trace("Execution started");
            var classroomRepository = new ClassroomRepository(_context);
            var plans = classroomRepository.ListNotConcludedDeliveryPlans();
            foreach (var item in plans)
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

            Logger.Trace("Execution ended");
        }
    }
}
