using System;
using System.Collections.Generic;

namespace BoltOn.Mediator
{
    public class MediatorOptions
    {
		internal List<Type> Middlewares { get; set; } = new List<Type>();
		public UnitOfWorkOptions UnitOfWorkOptions { get; set; }
		internal bool IsMiddlewaresCustomized { get; set; } 

        public MediatorOptions()
        {
			UnitOfWorkOptions = new UnitOfWorkOptions();
			Middlewares.Add(typeof(StopwatchMiddleware));
			Middlewares.Add(typeof(UnitOfWorkMiddleware));
        }

		public void RegisterMiddleware<TMiddleware>()
		{
			Middlewares.Add(typeof(TMiddleware));
			IsMiddlewaresCustomized = true;
		}

		public void ClearMiddlewares()
		{
			Middlewares.Clear();
			IsMiddlewaresCustomized = true;
		}
    }
}
