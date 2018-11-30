using System;
using System.Collections;
using BoltOn.Other;

namespace BoltOn.Context
{
	public interface IContextRetriever
	{
		TContext Get<TContext>(ContextScope contextScope = ContextScope.Request);
		void Set<TContext>(TContext context, ContextScope contextScope = ContextScope.Request);
	}

	[ExcludeFromRegistration]
	public sealed class ContextRetriever : IContextRetriever
	{
		private readonly Hashtable _contexts;
		private readonly IAppContextRetriever _appContextRetriever;

		internal ContextRetriever(IAppContextRetriever appContextRetriever)
		{
			_contexts = new Hashtable();
			_appContextRetriever = appContextRetriever;
		}

		public TContext Get<TContext>(ContextScope contextScope = ContextScope.Request)
		{
			if (contextScope == ContextScope.App)
				return _appContextRetriever.Get<TContext>();

			var context = _contexts[typeof(TContext).FullName];
			if (context == null)
				return default(TContext);

			return (TContext)Convert.ChangeType(context, typeof(TContext));
		}

		public void Set<TContext>(TContext context, ContextScope contextScope = ContextScope.Request)
		{
			if (contextScope == ContextScope.App)
				_appContextRetriever.Set<TContext>(context);

			_contexts[typeof(TContext).FullName] = context;
		}
	}
}
