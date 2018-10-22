using System;
using System.Collections;

namespace BoltOn
{
	public enum ContextScope
	{
		App,
		Request
	}

	//public interface IContextRetrieverFactory
	//{
	//	IContextRetriever Get();
	//}

	//public class ContextRetrieverFactory : IContextRetrieverFactory
	//{
	//	private readonly IAppContextRetriever _appContextRetriever;

	//	internal ContextRetrieverFactory(IAppContextRetriever appContextRetriever)
	//	{
	//		_appContextRetriever = appContextRetriever;
	//	}

	//	public IContextRetriever Get()
	//	{
	//		return new ContextRetriever(_appContextRetriever);
	//	}
	//}

	public interface IContextRetriever
	{
		TContext Get<TContext>(ContextScope contextScope = ContextScope.Request);
		void Set<TContext>(TContext context, ContextScope contextScope = ContextScope.Request);
	}

	internal interface IAppContextRetriever
	{
		TContext Get<TContext>();
		void Set<TContext>(TContext context);
	}

	internal sealed class AppContextRetriever : IAppContextRetriever
	{
		private readonly Hashtable _contexts;

		public AppContextRetriever()
		{
			_contexts = new Hashtable();
		}

		public TContext Get<TContext>()
		{
			var context = _contexts[typeof(TContext).FullName];
			if (context == null)
				return default(TContext);
			return (TContext)Convert.ChangeType(context, typeof(TContext));
		}

		public void Set<TContext>(TContext context)
		{
			_contexts[typeof(TContext).FullName] = context;
		}
	}

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
