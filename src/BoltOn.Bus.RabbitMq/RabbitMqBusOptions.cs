using System.Collections.Generic;
using System.Reflection;

namespace BoltOn.Bus.RabbitMq
{
	public class RabbitMqBusOptions
	{
		public string HostAddress { get; set; }

		public string Username { get; set; }

		public string Password { get; set; }

		public List<Assembly> AssembliesWithConsumers { get; set; } = new List<Assembly>();
	}
}
