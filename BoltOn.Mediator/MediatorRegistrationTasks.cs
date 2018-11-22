using System.Linq;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using System.Transactions;

namespace BoltOn.Mediator
{
	public class MediatorPreRegistrationTask : IBootstrapperPreRegistrationTask
    {
        public void Run(PreRegistrationTaskContext context)
        {
			context.Container.AddTransient<IMediatorMiddleware, StopwatchMiddleware>();
			context.Container.AddTransient<IMediatorMiddleware, UnitOfWorkMiddleware>();
		}
    }

	public class MediatorRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var serviceCollection = context.Container;
			serviceCollection.AddTransient<IMediator, Mediator>();

			context.Container.Configure<UnitOfWorkOptions>(u =>
			{
				u.DefaultCommandIsolationLevel = IsolationLevel.ReadCommitted;
				u.DefaultIsolationLevel = IsolationLevel.ReadCommitted;
				u.DefaultQueryIsolationLevel = IsolationLevel.ReadUncommitted;
			});

			RegisterHandlers(context);
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
