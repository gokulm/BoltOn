using System;
using System.Collections.Generic;
using System.Reflection;
using BoltOn.IoC;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
    public sealed class RegistrationTaskContext
    {
        private readonly Bootstrapper _bootstrapper;

        public RegistrationTaskContext(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        public IBoltOnContainer Container
        {
            get
            {
                return _bootstrapper.Container;
            }
        }

		public IServiceCollection ServiceCollection
		{
			get
			{
				return _bootstrapper.ServiceCollection;
			}
		}

		public IServiceProvider ServiceProvider
		{
			get
			{
				return _bootstrapper.ServiceProvider;
			}
		}

		public IReadOnlyCollection<Assembly> Assemblies
        {
            get
            {
                return _bootstrapper.Assemblies;
            }
        }

        public TOptionType GetOptions<TOptionType>() where TOptionType : class, new()
        {
            return _bootstrapper.GetOptions<TOptionType>();
        }
    }
}
