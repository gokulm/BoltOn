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
using BoltOn.Web.Filters;
using BoltOn.Web;
using BoltOn.Logging.Serilog;
using CorrelationId.DependencyInjection;
using Microsoft.OpenApi.Models;
using CorrelationId;

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
				options.BoltOnSerilogModule(Configuration);
				options.BoltOnAssemblies(typeof(PingHandler).Assembly, typeof(SchoolWriteDbContext).Assembly);
			});

			var writeDbConnectionString = Configuration.GetValue<string>("SqlWriteDbConnectionString");
			var readDbConnectionString = Configuration.GetValue<string>("SqlReadDbConnectionString");
			var redisUrl = Configuration.GetValue<string>("RedisUrl");

			services.AddDbContext<SchoolWriteDbContext>(options =>
			{
				options.UseSqlServer(writeDbConnectionString);
			});

			services.AddStackExchangeRedisCache(options =>
			{
				options.Configuration = redisUrl;
			});

			services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "BoltOn Samples", Version = "v1", }));

			services.AddControllers(c =>
			{
				c.Filters.Add<CustomExceptionFilter>();
				c.Filters.Add<ModelValidationFilter>();
			});
			services.AddTransient<IRepository<Student>, Repository<Student, SchoolWriteDbContext>>();
			services.AddTransient<IQueryRepository<StudentType>, QueryRepository<StudentType, SchoolWriteDbContext>>();
		}

		public void Configure(IApplicationBuilder app, IHostApplicationLifetime appLifetime, IWebHostEnvironment env)
		{
			app.UseCorrelationId();
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("../swagger/v1/swagger.json", "BoltOn Samples");
			});

			app.TightenBolts();
			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
			appLifetime.ApplicationStopping.Register(() => app.ApplicationServices.LoosenBolts());
		}
	}
}
