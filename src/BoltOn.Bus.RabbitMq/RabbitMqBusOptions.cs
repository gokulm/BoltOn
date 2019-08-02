using System;
using BoltOn.Mediator.Pipeline;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.RabbitMq
{
	public class RabbitMqBusOptions
	{
		public string HostAddress { get; set; }

		public string Username { get; set; }

		public string Password { get; set; }

		public IServiceCollection ServiceCollection { get; set; }

		public void AddConsumer<T>(IRabbitMqBusFactoryConfigurator configurator) where T : class, IMessage
		{
			var host = configurator.Host(new Uri("rabbitmq://localhost:5672/"), h =>
			{
				h.Username("guest");
				h.Password("guest");
			});

			ServiceCollection.AddTransient<T>((sp) =>
			{
				var mediator = sp.GetService<IMediator>();
				configurator.ReceiveEndpoint(host, $"{typeof(T).Name}_queue", endpoint =>
				{
					endpoint.Handler<T>(async context =>
					{
						await mediator.ProcessAsync(context.Message);
					});
				});
				return null;
			});
		}
	}
}
