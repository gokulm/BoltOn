using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Data.EF;
using BoltOn.Samples.Application.Handlers;
using Microsoft.EntityFrameworkCore;
using BoltOn.Samples.Infrastructure.Data;
using BoltOn.Samples.Infrastructure.Data.Repositories;
using BoltOn.Data.CosmosDb;
using BoltOn.Bus.MassTransit;
using MassTransit;
using System;
using BoltOn.Utilities;
using BoltOn.Data;
using BoltOn.Samples.Application.Entities;
using BoltOn.Bootstrapping;

namespace BoltOn.Samples.WebApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			services.BoltOn(options =>
			{
				options.BoltOnEFModule();
				options.BoltOnCosmosDbModule();
				options.BoltOnMassTransitBusModule();
				options.EnableCqrs();
				options.BoltOnAssemblies(typeof(PingHandler).Assembly, typeof(StudentRepository).Assembly);
			});
		}

		public void Configure(IApplicationBuilder app, IApplicationLifetime appLifetime)
		{
			app.UseMvc();
			app.ApplicationServices.TightenBolts();
			appLifetime.ApplicationStopping.Register(() => BoltOnAppCleaner.Clean());
		}
	}

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
				}));
			});

			container.AddDbContext<SchoolDbContext>(options =>
			{
				options.UseSqlServer("Data Source=127.0.0.1;initial catalog=Testing;persist security info=True;User ID=sa;Password=Password1;");
			});

			//services.AddCosmosDbContext<CollegeDbContext>(options =>
			//{
			//    options.Uri = "";
			//    options.AuthorizationKey = "";
			//    options.DatabaseName = "";
			//});

			container.AddTransient<IRepository<Student>, CqrsRepository<Student, SchoolDbContext>>();
			container.AddTransient<IRepository<StudentType>, Repository<StudentType, SchoolDbContext>>();
		}
	}
}
