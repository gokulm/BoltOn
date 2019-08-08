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
using BoltOn.Bus.RabbitMq;
using MassTransit;
using System;
using BoltOn.Samples.Application.Messages;

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
                options.BoltOnAssemblies(typeof(PingHandler).Assembly, typeof(StudentRepository).Assembly);
            });

			//services.BoltOnRabbitMqBus(o =>
			//{
			//	o.HostAddress = "rabbitmq://localhost:5672";
			//	o.Username = "guest";
			//	o.Password = "guest";
			//});

			var bus = MassTransit.Bus.Factory.CreateUsingRabbitMq(sbc =>
			{
				var host = sbc.Host(new Uri("rabbitmq://localhost"), h =>
				{
					h.Username("guest");
					h.Password("guest");
				});

				sbc.ReceiveEndpoint(host, "test_queue", ep =>
				{
					ep.Handler<CreateStudent>(context =>
					{
						return Console.Out.WriteLineAsync($"Received: test");
					});
				});


			});
			services.AddSingleton(bus);
			services.AddScoped<BoltOn.Bus.IBus, MassTransitBoltOnBus>();

			bus.Start(); // This is important!


			services.AddDbContext<SchoolDbContext>(options =>
            {
                options.UseSqlServer("Data Source=127.0.0.1;initial catalog=Testing;persist security info=True;User ID=sa;Password=$Password1;");
            });

            services.AddCosmosDbContext<CollegeDbContext>(options =>
            {
                options.Uri = "";
                options.AuthorizationKey = "";
                options.DatabaseName = "";
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
			var bus = app.ApplicationServices.GetService<IBusControl>();
			bus.Start();
			//app.ApplicationServices.UseRabbitMqBus(o =>
			//{
			//	o.HostAddress = "rabbitmq://localhost:5672";
			//	o.Username = "guest";
			//	o.Password = "guest";
			//}, typeof(CreateStudentHandler).Assembly);
            app.ApplicationServices.TightenBolts();
        }
    }
}
