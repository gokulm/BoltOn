using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Mediator.Pipeline;
using MassTransit;
using MassTransit.RabbitMqTransport;
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

		public static void BoltOnConsumer<TRequest>(this IRabbitMqBusFactoryConfigurator configurator, 
			IServiceProvider serviceProvider, IRabbitMqHost host, string queueName = null)
			 where TRequest : class, IRequest
		{
			configurator.ReceiveEndpoint(host, queueName ?? $"{typeof(TRequest).Name}_queue", endpoint =>
			{
				endpoint.Consumer(() => serviceProvider.GetService<MassTransitRequestConsumer<TRequest>>());
			});
		}
	}
}
