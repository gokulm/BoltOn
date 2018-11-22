using System;
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

		public IServiceCollection Container
		{
			get
			{
				return _bootstrapper.Container;
			}
		}

		public IReadOnlyCollection<Assembly> Assemblies
        {
            get
            {
                return _bootstrapper.Assemblies;
            }
        }
	}

	public sealed class PreRegistrationTaskContext
	{
		private readonly Bootstrapper _bootstrapper;

		internal PreRegistrationTaskContext(Bootstrapper bootstrapper)
		{
			_bootstrapper = bootstrapper;
		}

		public IServiceCollection Container
		{
			get
			{
				return _bootstrapper.Container;
			}
		}
	}
}
