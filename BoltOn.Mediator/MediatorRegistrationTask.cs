using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.IoC;

namespace BoltOn.Mediator
{
	public class MediatorRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var container = context.Container;
			container.RegisterTransient<IMediator, Mediator>();
			var options = context.GetOptions<MediatorOptions>();
			container.RegisterTransientCollection(typeof(IMediatorMiddleware), options.Middlewares);
			RegisterHandlers(container, context.Assemblies);
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
				container.RegisterTransient(handler.Interface, handler.Implementation);
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
