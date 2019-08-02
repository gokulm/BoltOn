using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Mediator.Pipeline;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.RabbitMq
{
	public static class Extensions
	{
		public static BoltOnOptions BoltOnRabbitMqBusModule(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
		}

		public static IServiceCollection BoltOnRabbitMqBus(this IServiceCollection serviceCollection, Action<RabbitMqBusOptions> action)
		{
			var options = new RabbitMqBusOptions();
			action(options);

			var busControl = MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
			{
				cfg.Host(new Uri(options.HostAddress), h =>
				{
					h.Username(options.Username);
					h.Password(options.Password);
				});
			});
			//busControl.Start();
			serviceCollection.AddSingleton(busControl);
			serviceCollection.AddScoped<IBus, MassTransitBus>();

			return serviceCollection;
		}

		public static IServiceProvider UseRabbitMqBus(this IServiceProvider serviceProvider, Action<RabbitMqBusOptions> action,
			Assembly assembly)
		{
			var options = new RabbitMqBusOptions();
			action(options);
			var mediator = serviceProvider.GetService<IMediator>();

			var types = from type in assembly.GetTypes()
						  where typeof(IMessage).IsAssignableFrom(type)
						  select type;


			var busControl = MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
			{
				var host = cfg.Host(new Uri(options.HostAddress), h =>
				{
					h.Username(options.Username);
					h.Password(options.Password);
				});

				foreach (var type in types)
				{
					var consumer = Activator.CreateInstance(typeof(MassTransitRequestConsumer<>)
																   .MakeGenericType(type), mediator) as IConsumer;
					cfg.ReceiveEndpoint(host, $"{type.Name}_queue", endpoint =>
					{
						endpoint.Instance(consumer);
					});
				}


			});
			busControl.Start();

			return serviceProvider;
		}

		internal class MassTransitRequestConsumer<TRequest> : IConsumer<TRequest> where TRequest : class, IMessage
		{
			private readonly IMediator _mediator;

			public MassTransitRequestConsumer(IMediator mediator)
			{
				_mediator = mediator;
			}

			public async Task Consume(ConsumeContext<TRequest> context)
			{
				var request = context.Message;
				await _mediator.ProcessAsync(request);
			}
		}
	}
}
