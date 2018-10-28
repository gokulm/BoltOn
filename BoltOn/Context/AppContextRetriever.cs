using System;
using System.Collections;

namespace BoltOn.Context
{
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
}
