using System.Linq;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using System.Transactions;

namespace BoltOn.Mediator
{
	public class MediatorRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var container = context.Container;
			container.AddTransient<IMediator, Mediator>();
			RegisterMiddlewares(container);
			RegisterHandlers(context);
			Configure(container);
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

		private static void Configure(IServiceCollection container)
		{
			container.Configure<UnitOfWorkOptions>(u =>
			{
				u.DefaultCommandIsolationLevel = IsolationLevel.ReadCommitted;
				u.DefaultIsolationLevel = IsolationLevel.ReadCommitted;
				u.DefaultQueryIsolationLevel = IsolationLevel.ReadUncommitted;
			});
		}
	}
}
