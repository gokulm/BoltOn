using System.Linq;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using System.Transactions;
using BoltOn.Mediator.Middlewares;

namespace BoltOn.Mediator
{
	public class MediatorRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var container = context.Container;
			container.AddTransient<IMediator, Mediator>();
			container.AddSingleton<IUnitOfWorkOptionsRetriever, UnitOfWorkOptionsRetriever>();
			RegisterMiddlewares(container);
			RegisterHandlers(context);
		}

		private static void RegisterMiddlewares(IServiceCollection container)
		{
			container.AddTransient<IMediatorMiddleware, StopwatchMiddleware>();
			container.AddTransient<IMediatorMiddleware, UnitOfWorkMiddleware>();
		}

		private void RegisterHandlers(RegistrationTaskContext context)
		{
			var requestHandlerInterfaceType = typeof(IRequestHandler<,>);
			var handlers = (from a in context.Assemblies.ToList()
							from t in a.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType &&
								requestHandlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
							select new { Interface = i, Implementation = t }).ToList();
			foreach (var handler in handlers)
				context.Container.AddTransient(handler.Interface, handler.Implementation);
		}
	}
}
