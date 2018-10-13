using System.Collections.Generic;
using System.Reflection;
using BoltOn.IoC;

namespace BoltOn.Bootstrapping
{
    public interface IBootstrapperRegistrationTask
    {
		void Run(RegistrationTaskContext context);
    }

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

		public IReadOnlyCollection<Assembly> Assemblies
		{
			get
			{
				return _bootstrapper.Assemblies;
			}
		}

		public TOptionType GetOptions<TOptionType>() where TOptionType : class
		{
			return _bootstrapper.GetOptions<TOptionType>();
		}
	}
}
