using NLog;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Repositories;
using System;
using System.Collections.Generic;

namespace SmartLMS.ContentDeliveryAgent
{
    public class ContentDeliveryService
    {
        public static List<Parameter> Parameters { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IContext _context;
        public ContentDeliveryService(IContext context)
        {
            _context = context;
        }

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
