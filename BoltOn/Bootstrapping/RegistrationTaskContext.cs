using System;
using System.Collections.Generic;
using System.Reflection;
using BoltOn.Utilities;
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

		public IServiceCollection ServiceCollection
		{
			get
			{
				return _bootstrapper.ServiceCollection;
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

		public void AddOptions<TOptionType>(TOptionType options) where TOptionType : class
		{
			_bootstrapper.AddOptions(options);
		}
	}

	public sealed class PreRegistrationTaskContext
	{
		private readonly Bootstrapper _bootstrapper;

		internal PreRegistrationTaskContext(Bootstrapper bootstrapper)
		{
			_bootstrapper = bootstrapper;
		}

		public IServiceCollection ServiceCollection
		{
			get
			{
				return _bootstrapper.ServiceCollection;
			}
		}

		public void Configure<TOptionType>(Action<TOptionType> action) where TOptionType : class, new()
		{
			//Check.Requires(!_bootstrapper.IsBolted, "Components are already bolted! Options cannot be added");
			var options = new TOptionType();
			action(options);
			_bootstrapper.AddOptions(options);
		}
	}
}
