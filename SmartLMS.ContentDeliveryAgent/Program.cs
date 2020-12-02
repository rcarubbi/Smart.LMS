﻿using Quartz;
using SmartLMS.DAL;
using StructureMap;
using Topshelf;
using Topshelf.Quartz.StructureMap;
using Topshelf.StructureMap;
using IContext = SmartLMS.Domain.IContext;

namespace SmartLMS.ContentDeliveryAgent
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(serviceConfig =>
            {
                serviceConfig.UseNLog();

                var container = new Container(cfg => { cfg.For<IContext>().Use<Context>(); });

                serviceConfig.UseStructureMap(container);

                serviceConfig.Service<ContentDeliveryService>(serviceInstance =>
                {
                    serviceInstance.ConstructUsingStructureMap();
                    serviceInstance.WhenStarted(execute => execute.Start());
                    serviceInstance.WhenStopped(execute => execute.Stop());

                    serviceInstance.UseQuartzStructureMap();

                    serviceInstance.ScheduleQuartzJob(q =>
                        q.WithJob(() =>
                                JobBuilder.Create<ContentDeliveryJob>().Build())
                            //.AddTrigger(() =>
                            //    TriggerBuilder.Create().StartNow().Build())
                            .AddTrigger(() =>
                                TriggerBuilder.Create()
                                    .WithSimpleSchedule(builder => builder
                                        .WithIntervalInHours(24)
                                        .RepeatForever())
                                    .Build())
                    );
                });

                serviceConfig.EnableServiceRecovery(recoveryOption => { recoveryOption.RestartService(5); });

                serviceConfig.SetServiceName("SmartLMS.ContentDeliveryAgent");
                serviceConfig.SetDisplayName("Smart LMS - Content Delivery Agent");
                serviceConfig.SetDescription("This agent is responsable to deliver new classes to students");
                serviceConfig.StartAutomatically();
            });
        }
    }
}