using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.IoC;

namespace BoltOn.Mediator
{
	public class HandlersRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(IBoltOnContainer container, IEnumerable<Assembly> assemblies)
		{
			var requestHandlerInterfaceType = typeof(IRequestHandler<,>);
			var registrationTaskTypes = (from a in assemblies.ToList()
										 from t in a.GetTypes()
			                             where 
			                             //requestHandlerInterfaceType.IsAssignableFrom(t) && 
			                             t.IsClass
										 select t).ToList();
		}
	}
}
