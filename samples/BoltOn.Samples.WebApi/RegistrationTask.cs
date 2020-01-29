using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BoltOn.Samples.Infrastructure.Data;
using MassTransit;
using System;
using BoltOn.Data;
using BoltOn.Samples.Application.Entities;
using BoltOn.Bootstrapping;

namespace BoltOn.Samples.WebApi
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
                }));
            });

            serviceCollection.AddDbContext<SchoolDbContext>(options =>
            {
                options.UseSqlServer("Data Source=bolton-sql-container;initial catalog=BoltOnSamples;persist security info=True;User ID=sa;Password=Password1;");
            });

			serviceCollection.AddTransient<IRepository<Student>, Data.EF.Repository<Student, SchoolDbContext>>();
			serviceCollection.AddTransient<IRepository<StudentType>, Data.EF.Repository<StudentType, SchoolDbContext>>();
			serviceCollection.AddTransient<IRepository<StudentFlattened>, Data.EF.Repository<StudentFlattened, SchoolDbContext>>();
		}
    }
}
