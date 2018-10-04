using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using BoltOn.Bootstrapping;

namespace BoltOn.Mediator
{
	public interface IMiddleware
	{
		StandardDtoReponse<TResponse> Run<TRequest, TResponse>(IRequest<TResponse> request, Func<IRequest<TResponse>,
										  StandardDtoReponse<TResponse>> next)
			where TRequest : IRequest<TResponse>;
	}

	public class MediatorOptions
	{
		public List<Type> Middlewares { get; set; } = new List<Type>();

		public MediatorOptions()
		{
			Middlewares.Add(typeof(StopwatchMiddleware));
		}
	}

	public static class MediatorExtensions
	{
		public static void BoltOnMediator(this Bootstrapper bootstrapper, Action<MediatorOptions> optionsAction = null)
		{
			Contract.Requires(bootstrapper.Container != null, "Bolt on DI before bolting on Mediator");
			var newOptions = new MediatorOptions();
			optionsAction?.Invoke(newOptions);
			bootstrapper.Container.RegisterScoped<IMediator, Mediator>();
			bootstrapper.Container.RegisterTransientCollection(typeof(IMiddleware), newOptions.Middlewares);
			RegisterHandlers(bootstrapper);
		}

		private static void RegisterHandlers(Bootstrapper bootstrapper)
		{
			var requestHandlerInterfaceType = typeof(IRequestHandler<,>);
			var handlers = (from a in bootstrapper.Assemblies.ToList()
							from t in a.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType &&
								requestHandlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
							select new { Interface = i, Implementation = t }).ToList();
			foreach (var handler in handlers)
				bootstrapper.Container.RegisterTransient(handler.Interface, handler.Implementation);
		}
	}
}