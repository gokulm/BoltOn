using BoltOn.IoC;

namespace BoltOn.Bootstrapping
{
    public interface IBootstrapperRegistrationTask
    {
        void Run(IBoltOnContainer container);
    }
}
