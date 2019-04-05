using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public sealed class RegistrationTaskContext
	{
		private readonly Bootstrapper _bootstrapper;

		internal RegistrationTaskContext(Bootstrapper bootstrapper)
		{
			_bootstrapper = bootstrapper;
		}

		public IServiceCollection Container => _bootstrapper.Container;

	    public IReadOnlyList<Assembly> Assemblies => _bootstrapper.Assemblies;
	}
}
