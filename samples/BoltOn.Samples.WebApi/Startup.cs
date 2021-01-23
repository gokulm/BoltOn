using BoltOn.Cache;
using BoltOn.Data;
using BoltOn.Data.EF;
using BoltOn.Logging.Serilog;
using BoltOn.Samples.Application.Entities;
using BoltOn.Samples.Application.Handlers;
using BoltOn.Samples.Infrastructure.Data;
using BoltOn.Web;
using BoltOn.Web.Filters;
using CorrelationId;
using CorrelationId.DependencyInjection;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

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
				options.BoltOnAssemblies(typeof(PingHandler).Assembly, typeof(SchoolDbContext).Assembly);
			});

			var writeDbConnectionString = Configuration.GetValue<string>("SqlWriteDbConnectionString");
			var redisUrl = Configuration.GetValue<string>("RedisUrl");

			services.AddDbContext<SchoolDbContext>(options =>
			{
				options.UseSqlServer(writeDbConnectionString);
			});

			services.AddDistributedMemoryCache();

			services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "BoltOn Samples", Version = "v1", }));

			services.AddControllers(c =>
			{
				c.Filters.Add<CustomExceptionFilter>();
				c.Filters.Add<ModelValidationFilter>();
			});
			services.AddTransient<IRepository<Student>, Repository<Student, SchoolDbContext>>();
			services.AddTransient<IQueryRepository<StudentType>, QueryRepository<StudentType, SchoolDbContext>>();
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
