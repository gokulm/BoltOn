using BoltOn.Samples.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bus.MassTransit;
using MassTransit;
using System;
using BoltOn.Data;
using BoltOn.Samples.Application.Entities;
using BoltOn.Samples.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BoltOn.Bootstrapping;

namespace BoltOn.Samples.Console
{
	public class RegistrationTask : IRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            var serviceCollection = context.ServiceCollection;

            serviceCollection.AddMassTransit(x =>
            {
                x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var host = cfg.Host(new Uri("rabbitmq://bolton-rabbitmq-container:5672"), hostConfigurator =>
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

            serviceCollection.AddDbContext<SchoolDbContext>(options =>
            {
                options.UseSqlServer("Data Source=bolton-sql-container;initial catalog=BoltOnSamples;persist security info=True;User ID=sa;Password=Password1;");
            });

			serviceCollection.AddTransient<IRepository<Student>, Data.EF.Repository<Student, SchoolDbContext>>();
			serviceCollection.AddTransient<IRepository<StudentFlattened>, Data.EF.Repository<StudentFlattened, SchoolDbContext>>();
		}
    }
}
