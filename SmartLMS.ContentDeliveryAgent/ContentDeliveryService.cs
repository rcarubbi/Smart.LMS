using System;
using System.Collections.Generic;
using NLog;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities;

namespace SmartLMS.ContentDeliveryAgent
{
    public class ContentDeliveryService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IContext _context;

        public ContentDeliveryService(IContext context)
        {
            _context = context;
        }

        public static List<Parameter> Parameters { get; set; }

        public bool Start()
        {
            Parameter.LoadParameters(_context);
            Console.WriteLine($"{Parameter.APP_NAME} - Content access agent started");
            Logger.Trace("Service started");
            return true;
        }

        public bool Stop()
        {
            Logger.Trace("Service ended");
            return true;
        }
    }
}