using BoltOn.Bootstrapping;

namespace BoltOn.IoC
{
    public interface IBoltOnContainerFactory
    {
        IBoltOnContainer Create();
    }
}
