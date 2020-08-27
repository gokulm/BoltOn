using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Data.EF;
using BoltOn.Samples.Application.Handlers;
using BoltOn.Bus.MassTransit;
using BoltOn.Samples.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using System;
using Microsoft.Extensions.Hosting;
using BoltOn.Data;
using BoltOn.Samples.Application.Entities;
using Microsoft.AspNetCore.Hosting;
using BoltOn.Cache;
using Hangfire;
using Hangfire.SqlServer;

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
			services.BoltOn(options =>
			{
				options.BoltOnEFModule();
				options.BoltOnMassTransitBusModule();
				options.BoltOnCqrsModule();
				options.BoltOnCacheModule();
				options.BoltOnAssemblies(typeof(PingHandler).Assembly, typeof(SchoolWriteDbContext).Assembly);
			});

			var writeDbConnectionString = Configuration.GetValue<string>("SqlWriteDbConnectionString");
            var readDbConnectionString = Configuration.GetValue<string>("SqlReadDbConnectionString");
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

            services.AddDbContext<SchoolWriteDbContext>(options =>
            {
                options.UseSqlServer(writeDbConnectionString);
            });

            services.AddDbContext<SchoolReadDbContext>(options =>
            {
                options.UseSqlServer(readDbConnectionString);
            });

			services.AddStackExchangeRedisCache(options =>
			{
				options.Configuration = redisUrl;
			});

			services.AddControllers();

			services.AddHangfire(configuration => configuration
				.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
				.UseSimpleAssemblyNameTypeSerializer()
				.UseRecommendedSerializerSettings()
				.UseSqlServerStorage("Data Source=127.0.0.1,5005;initial catalog=HangfireTest;persist security info=True;User ID=sa;Password=Password1;", new SqlServerStorageOptions
				{
					CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
					SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
					QueuePollInterval = TimeSpan.Zero,
					UseRecommendedIsolationLevel = true,
					DisableGlobalLocks = true
				}));


			services.AddTransient<IRepository<Student>, CqrsRepository<Student, SchoolWriteDbContext>>();
			services.AddTransient<IRepository<StudentType>, Repository<StudentType, SchoolWriteDbContext>>();
			services.AddTransient<IRepository<StudentFlattened>, CqrsRepository<StudentFlattened, SchoolReadDbContext>>();
		}

		public void Configure(IApplicationBuilder app, IHostApplicationLifetime appLifetime, IWebHostEnvironment env)
		{
            app.ApplicationServices.TightenBolts();
			GlobalConfiguration.Configuration
				.UseActivator(new HangfireActivator(app.ApplicationServices));
			app.UseHangfireDashboard();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
			appLifetime.ApplicationStopping.Register(() => app.ApplicationServices.LoosenBolts());
		}
	}

	public class HangfireActivator : JobActivator
	{
		private readonly IServiceProvider _serviceProvider;

		public HangfireActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public override object ActivateJob(Type type)
		{
			return _serviceProvider.GetService(type);
		}
	}
}
