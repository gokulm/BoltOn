using System;
using System.Collections.Generic;
using System.Reflection;
using BoltOn.Mediator.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public sealed class RegistrationTaskContext
	{
		private readonly Bootstrapper _bootstrapper;
		internal HashSet<Type> InterceptorTypes { get; private set; }

		internal RegistrationTaskContext(Bootstrapper bootstrapper)
		{
			_bootstrapper = bootstrapper;
			InterceptorTypes = new HashSet<Type>();
		}

		public IServiceCollection Container => _bootstrapper.Container;

	    public IReadOnlyList<Assembly> Assemblies => _bootstrapper.Assemblies;

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
