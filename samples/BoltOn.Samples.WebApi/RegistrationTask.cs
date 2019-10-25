using Microsoft.Extensions.DependencyInjection;
using BoltOn.Data.EF;
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
        void IRegistrationTask.Run(RegistrationTaskContext context)
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
                }));
            });

            container.AddDbContext<SchoolDbContext>(options =>
            {
                options.UseSqlServer("Data Source=127.0.0.1;initial catalog=BoltOnSamples;persist security info=True;User ID=sa;Password=Password1;");
            });

            container.AddCosmosDb<SchoolCosmosDbOptions>(options =>
            {
                options.Uri = "https://bolton2.documents.azure.com:443/";
                options.AuthorizationKey = "CJNc3RPjK3ACzRBtjOg56rJ774Y3ncyvJKCl5X2pfpMVe5wLPkr2v80pN5wWjhmZXYA0blOEsIDT4MmQifjtrg==";
                options.DatabaseName = "School";
            });

            container.AddTransient<IRepository<Student>, CqrsRepository<Student, SchoolDbContext>>();
            container.AddTransient<IRepository<StudentType>, Data.EF.Repository<StudentType, SchoolDbContext>>();
			container.AddTransient<IRepository<StudentFlattened>, Data.CosmosDb.Repository<StudentFlattened, SchoolCosmosDbOptions>>();
			//container.AddTransient<IRepository<StudentFlattened>, CqrsRepository<StudentFlattened, SchoolDbContext>>();
        }
    }
}
