using System;
using System.Collections.Generic;
using System.Linq;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Mediator
{
	public class MediatorRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var serviceCollection = context.ServiceCollection;
			serviceCollection.AddTransient<IMediator, Mediator>();

			var options = context.GetOptions<MediatorOptions>();
			//if (!options.IsMiddlewaresCustomized)
			//{
			//	RegisterMiddlewares(context, options.Middlewares);
			//}
			//else
			//{
				options.Middlewares.ForEach(m => serviceCollection.AddTransient(typeof(IMediatorMiddleware), m));
			//}
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
			defaultMiddlewareTypes.ForEach(m => context.ServiceCollection.AddTransient(typeof(IMediatorMiddleware), m));
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
}
