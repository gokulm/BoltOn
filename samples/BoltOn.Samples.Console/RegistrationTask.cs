using BoltOn.Samples.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bus.MassTransit;
using MassTransit;
using System;
using BoltOn.Data;
using BoltOn.Samples.Application.Entities;
using BoltOn.Data.EF;
using BoltOn.Samples.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BoltOn.Bootstrapping;

namespace BoltOn.Samples.Console
{
    public class RegistrationTask : IRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            var container = context.Container;

            container.AddMassTransit(x =>
            {
                x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var host = cfg.Host(new Uri("rabbitmq://localhost:5672"), hostConfigurator =>
                    {
                        hostConfigurator.Username("guest");
                        hostConfigurator.Password("guest");
                    });

                    cfg.ReceiveEndpoint("StudentCreatedEvent_queue", ep =>
                    {
                        ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<StudentCreatedEvent>>());
                    });

                    cfg.ReceiveEndpoint("StudentUpdatedEvent_queue", ep =>
                    {
                        ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<StudentUpdatedEvent>>());
                    });
                }));
            });

            container.AddDbContext<SchoolDbContext>(options =>
            {
                options.UseSqlServer("Data Source=127.0.0.1;initial catalog=Testing;persist security info=True;User ID=sa;Password=Password1;");
            });

            container.AddTransient<IRepository<StudentFlattened>, CqrsRepository<StudentFlattened, SchoolDbContext>>();
        }
    }
}
