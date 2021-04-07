using System;
using AutoMapper;
using BoltOn.Cache;
using BoltOn.Data;
using BoltOn.Data.EF;
using BoltOn.Samples.Application;
using BoltOn.Samples.Application.Entities;
using BoltOn.Samples.Application.Handlers;
using BoltOn.Samples.Infrastructure.Data;
using BoltOn.Web;
using BoltOn.Web.Filters;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Hangfire;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using BoltOn.Bus.MassTransit;

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
			services.AddDefaultCorrelationId();
			services.BoltOn(options =>
			{
				options.BoltOnEFModule();
				options.BoltOnCacheModule();
				options.BoltOnWebModule();
				options.BoltOnMassTransitBusModule();
				options.BoltOnAssemblies(typeof(PingHandler).Assembly, typeof(SchoolDbContext).Assembly);
			});

			var boltOnSamplesDbConnectionString = Configuration.GetValue<string>("BoltOnSamplesDbConnectionString");

			services.AddDbContext<SchoolDbContext>(options =>
			{
				options.UseSqlServer(boltOnSamplesDbConnectionString);
			});

			services.AddDistributedMemoryCache();
			services.AddAutoMapper(typeof(MappingProfile));

			var rabbitmqUri = Configuration.GetValue<string>("RabbitMqUri");
			var rabbitmqUsername = Configuration.GetValue<string>("RabbitMqUsername");
			var rabbitmqPassword = Configuration.GetValue<string>("RabbitMqPassword");
			var redisUrl = Configuration.GetValue<string>("RedisUrl");
			services.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
				{
					cfg.Host(new Uri(rabbitmqUri), hostConfigurator =>
					{
						hostConfigurator.Username(rabbitmqUsername);
						hostConfigurator.Password(rabbitmqPassword);
					});
				}));
			});


			if (Configuration.GetValue<bool>("IsHangfireEnabled"))
			{
				GlobalConfiguration.Configuration
				 .UseSqlServerStorage(boltOnSamplesDbConnectionString);

				services.AddHangfire(config =>
				{
					config.UseSqlServerStorage(boltOnSamplesDbConnectionString);
				});
			}

			Log.Logger = new LoggerConfiguration()
							.Enrich.WithMachineName()
							.Enrich.FromLogContext()
							.ReadFrom.Configuration(Configuration)
							.CreateLogger();

			services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "BoltOn Samples", Version = "v1", }));

			services.AddControllers(c =>
			{
				c.Filters.Add<CustomExceptionFilter>();
				c.Filters.Add<ModelValidationFilter>();
			});
			services.AddTransient<IRepository<Student>, CqrsRepository<Student, SchoolDbContext>>();
			services.AddTransient<IQueryRepository<StudentType>, QueryRepository<StudentType, SchoolDbContext>>();
			services.AddTransient<IQueryRepository<Course>, QueryRepository<Course, SchoolDbContext>>();
		}

		public void Configure(IApplicationBuilder app, IHostApplicationLifetime appLifetime, IWebHostEnvironment env)
		{
			app.UseCorrelationId();
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("../swagger/v1/swagger.json", "BoltOn Samples");
				c.DefaultModelsExpandDepth(-1);
			});

			app.TightenBolts();

			if (Configuration.GetValue<bool>("IsHangfireEnabled"))
				app.UseHangfireDashboard("/hangfire");

			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
			appLifetime.ApplicationStopping.Register(() => app.ApplicationServices.LoosenBolts());
		}
	}
}
