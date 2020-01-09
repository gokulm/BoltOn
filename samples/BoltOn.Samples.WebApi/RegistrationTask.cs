using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BoltOn.Samples.Infrastructure.Data;
using MassTransit;
using System;
using BoltOn.Data;
using BoltOn.Samples.Application.Entities;
using BoltOn.Bootstrapping;
using BoltOn.Data.CosmosDb;

namespace BoltOn.Samples.WebApi
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
                    var host = cfg.Host(new Uri("rabbitmq://bolton-rabbitmq-container:5010"), hostConfigurator =>
                    {
                        hostConfigurator.Username("guest");
                        hostConfigurator.Password("guest");
                    });
                }));
            });

            container.AddDbContext<SchoolDbContext>(options =>
            {
                options.UseSqlServer("Data Source=bolton-sql-container;initial catalog=BoltOnSamples;persist security info=True;User ID=sa;Password=Password1;");
            });

			// container.AddCosmosDb<SchoolCosmosDbOptions>(options =>
			// {
			// 	options.Uri = "https://bolton.documents.azure.com:443/";
			// 	options.AuthorizationKey = "XZZAFWzdJoqG5IoJGUHIFGoYMP4rCof5o60wbMSIyzEZBwID4POEmCDRLUNscPh2K9VcV0Ccm7aGsLnvccGj7A==";
			// 	options.DatabaseName = "School";
			// });

			container.AddTransient<IRepository<Student>, Data.EF.Repository<Student, SchoolDbContext>>();
			container.AddTransient<IRepository<StudentType>, Data.EF.Repository<StudentType, SchoolDbContext>>();
			container.AddTransient<IRepository<StudentFlattened>, Data.EF.Repository<StudentFlattened, SchoolDbContext>>();
			//container.AddTransient<IRepository<StudentFlattened>, Data.CosmosDb.Repository<StudentFlattened, SchoolCosmosDbOptions>>();
		}
    }
}
