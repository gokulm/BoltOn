using System.Collections.Generic;
using System.Reflection;
using BoltOn.IoC;

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
