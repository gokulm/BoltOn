using System;
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
			if (!options.IsMiddlewaresCustomized)
				RegisterMiddlewares(context, options.Middlewares);
			else
				container.RegisterTransientCollection(typeof(IMediatorMiddleware), options.Middlewares);
			RegisterHandlers(context);
		}

		private void RegisterMiddlewares(RegistrationTaskContext context, List<Type> defaultMiddlewareTypes)
		{
			var mediatorMiddlewareType = typeof(IMediatorMiddleware);
			var middlewares = (from a in context.Assemblies.ToList()
							   where a.FullName != this.GetType().Assembly.FullName
							   from t in a.GetTypes()
							   where mediatorMiddlewareType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract
							   select t).ToList();
			defaultMiddlewareTypes.AddRange(middlewares);
			context.Container.RegisterTransientCollection(typeof(IMediatorMiddleware), defaultMiddlewareTypes);
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
				context.Container.RegisterTransient(handler.Interface, handler.Implementation);
		}
	}
}
