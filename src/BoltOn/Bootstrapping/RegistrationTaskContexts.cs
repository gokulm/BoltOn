using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
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

		public IReadOnlyList<Assembly> Assemblies
		{
			get
			{
				return _bootstrapper.Assemblies;
			}
		}
	}

	public sealed class PostRegistrationTaskContext
	{
		private readonly Bootstrapper _bootstrapper;

		internal PostRegistrationTaskContext(Bootstrapper bootstrapper)
		{
			_bootstrapper = bootstrapper;
		}

		public IServiceProvider ServiceProvider
		{
			get
			{
				return _bootstrapper.ServiceProvider;
			}
		}

		public IReadOnlyList<Assembly> Assemblies
		{
			get
			{
				return _bootstrapper.Assemblies;
			}
		}
	}
}
