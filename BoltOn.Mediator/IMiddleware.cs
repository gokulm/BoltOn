using System;
using System.Collections.Generic;

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
}