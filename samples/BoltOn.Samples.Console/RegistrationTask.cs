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
using BoltOn.Data.CosmosDb;
using BoltOn.Cqrs;

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
                options.UseSqlServer("Data Source=127.0.0.1;initial catalog=BoltOnSamples;persist security info=True;User ID=sa;Password=Password1;");
            });

			container.AddCosmosDb<SchoolCosmosDbOptions>(options =>
			{
				options.Uri = "https://bolton.documents.azure.com:443/";
				options.AuthorizationKey = "XZZAFWzdJoqG5IoJGUHIFGoYMP4rCof5o60wbMSIyzEZBwID4POEmCDRLUNscPh2K9VcV0Ccm7aGsLnvccGj7A==";
				options.DatabaseName = "School";
			});

			container.AddTransient<IRepository<Student>, Data.EF.Repository<Student, SchoolDbContext>>();
			container.AddTransient<IRepository<StudentFlattened>, Data.EF.Repository<StudentFlattened, SchoolDbContext>>();
			//container.AddTransient<IRepository<StudentFlattened>, Data.CosmosDb.Repository<StudentFlattened, SchoolCosmosDbOptions>>();
		}
    }
}
