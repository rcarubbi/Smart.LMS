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

        public bool Iniciar() {
            var parameterRepository = new ParameterRepository(_context);

            Parameter.APP_NAME = parameterRepository.ObterValorPorChave(Parameter.APP_NAME_KEY);
            Parameter.KNOWLEDGE_AREA_PLURAL = parameterRepository.ObterValorPorChave(Parameter.KNOWLEDGE_AREA_PLURAL_KEY);
            Parameter.KNOWLEDGE_AREA = parameterRepository.ObterValorPorChave(Parameter.KNOWLEDGE_AREA_KEY);
            Parameter.COURSE_PLURAL = parameterRepository.ObterValorPorChave(Parameter.COURSE_PLURAL_KEY);
            Parameter.COURSE = parameterRepository.ObterValorPorChave(Parameter.COURSE_KEY);
            Parameter.SUBJECT_PLURAL = parameterRepository.ObterValorPorChave(Parameter.SUBJECT_PLURAL_KEY);
            Parameter.SUBJECT = parameterRepository.ObterValorPorChave(Parameter.SUBJECT_KEY);
            Parameter.CLASS = parameterRepository.ObterValorPorChave(Parameter.CLASS_KEY);
            Parameter.CLASS_PLURAL = parameterRepository.ObterValorPorChave(Parameter.CLASS_PLURAL_KEY);
            Parameter.FILE = parameterRepository.ObterValorPorChave(Parameter.FILE_KEY);
            Parameter.WATCHED_CLASSES_TITLE = parameterRepository.ObterValorPorChave(Parameter.WATCHED_CLASSES_TITLE_KEY);
            Parameter.FILE_STORAGE = parameterRepository.ObterValorPorChave(Parameter.FILE_STORAGE_KEY);
            Parameter.LAST_CLASSES_TITLE = parameterRepository.ObterValorPorChave(Parameter.LAST_CLASSES_TITLE_KEY);
            Parameter.DELIVERED_CLASS_NOTICE_BODY  = parameterRepository.ObterValorPorChave(Parameter.DELIVERED_CLASS_NOTICE_BODY_KEY);

            Console.WriteLine($"{Parameter.APP_NAME} - Content access agent started");
            Logger.Trace("Service started");
            return true;
        }

        public bool Parar() {
            Logger.Trace("Service ended");
            return true;
        }

    }
}
