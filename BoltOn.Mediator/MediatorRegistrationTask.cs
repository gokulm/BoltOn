using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.IoC;

namespace BoltOn.Mediator
{
	public class MediatorRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(IBoltOnContainer container, IEnumerable<Assembly> assemblies)
		{
			container.RegisterScoped<IMediator, Mediator>();
			//container.RegisterTransient<Mediator>();
			RegisterHandlers(container, assemblies);
		}

		private void RegisterHandlers(IBoltOnContainer container, IEnumerable<Assembly> assemblies)
		{
			var requestHandlerInterfaceType = typeof(IRequestHandler<,>);
			var handlers = (from a in assemblies.ToList()
							from t in a.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType &&
								requestHandlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
							select new { Interface = i, Implementation = t }).ToList();

			foreach (var handler in handlers)
			{
				container.RegisterTransient(handler.Interface, handler.Implementation);
			}
		}
	}
}
