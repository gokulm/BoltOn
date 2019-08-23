using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Mediator.Pipeline;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace BoltOn.Bus.RabbitMq
{
	public static partial class Extensions
	{
		public static BoltOnOptions BoltOnRabbitMqBusModule(this BoltOnOptions boltOnOptions)
		{
			boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
			return boltOnOptions;
		}

		public static void BoltOnConsumer<TRequest>(this IRabbitMqBusFactoryConfigurator configurator, IRabbitMqHost host)
			 where TRequest : class, IRequest
		{
			// todo (med): support passing queue name convention or name generator as param		
			configurator.ReceiveEndpoint(host, $"{typeof(TRequest).Name}_queue", endpoint =>
			{
				endpoint.Consumer<MassTransitRequestConsumer<TRequest>>();
			});
		}
	}
}
