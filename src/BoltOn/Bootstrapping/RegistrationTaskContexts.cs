using System;
using System.Collections.Generic;
using System.Reflection;
using BoltOn.Mediator.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public sealed class RegistrationTaskContext
	{
		internal Bootstrapper Bootstrapper { get; }
		internal HashSet<Type> InterceptorTypes { get; }

		internal RegistrationTaskContext(Bootstrapper bootstrapper)
		{
			Bootstrapper = bootstrapper;
			InterceptorTypes = new HashSet<Type>();
		}

		public IServiceCollection ServiceCollection => Bootstrapper.ServiceCollection;

	    public IReadOnlyList<Assembly> Assemblies => Bootstrapper.Assemblies;

		public void AddInterceptor<TInterceptor>() where TInterceptor : IInterceptor
		{
			InterceptorTypes.Add(typeof(TInterceptor));
		}

		public void RemoveInterceptor<TInterceptor>() where TInterceptor : IInterceptor
		{
			InterceptorTypes.Remove(typeof(TInterceptor));
		}

		public void RemoveAllInterceptors()
		{
			InterceptorTypes.Clear();
		}
	}
}
