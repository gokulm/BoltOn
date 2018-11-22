using System.Linq;
using BoltOn.Bootstrapping;
using BoltOn.Context;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Mediator
{
    public class MediatorPreRegistrationTask : IBootstrapperPreRegistrationTask
    {
        public void Run(PreRegistrationTaskContext context)
        {
			context.ServiceCollection.AddTransient<IMediatorMiddleware, StopwatchMiddleware>();
			context.ServiceCollection.AddTransient<IMediatorMiddleware, UnitOfWorkMiddleware>();
		}
    }

	public class MediatorRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var serviceCollection = context.ServiceCollection;
			serviceCollection.AddTransient<IMediator, Mediator>();

			var mediatorOptions = context.GetOptions<MediatorOptions>();
			//mediatorOptions.Middlewares.ForEach(m => serviceCollection.AddTransient(typeof(IMediatorMiddleware), m));

			context.ServiceCollection.Configure<UnitOfWorkOptions>(u =>
			{
				u.DefaultCommandIsolationLevel = mediatorOptions.UnitOfWorkOptions.DefaultCommandIsolationLevel;
				u.DefaultIsolationLevel = mediatorOptions.UnitOfWorkOptions.DefaultIsolationLevel;
				u.DefaultQueryIsolationLevel = mediatorOptions.UnitOfWorkOptions.DefaultQueryIsolationLevel;
				u.DefaultTransactionTimeout = mediatorOptions.UnitOfWorkOptions.DefaultTransactionTimeout;
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
				context.ServiceCollection.AddTransient(handler.Interface, handler.Implementation);
		}
	}


	public class MediatorPostRegistrationTask : IBootstrapperPostRegistrationTask
	{
		private readonly IContextRetriever _contextRetriever;

		public MediatorPostRegistrationTask(IContextRetriever contextRetriever)
		{
			_contextRetriever = contextRetriever;
		}

		public void Run(RegistrationTaskContext context)
		{
			var options = context.GetOptions<MediatorOptions>();
			var mediatorContext = new MediatorContext
			{
				DefaultCommandIsolationLevel = options.UnitOfWorkOptions.DefaultCommandIsolationLevel,
				DefaultQueryIsolationLevel = options.UnitOfWorkOptions.DefaultQueryIsolationLevel,
				DefaultIsolationLevel = options.UnitOfWorkOptions.DefaultIsolationLevel
			};

			_contextRetriever.Set(mediatorContext, ContextScope.App);
		}
	}
}
