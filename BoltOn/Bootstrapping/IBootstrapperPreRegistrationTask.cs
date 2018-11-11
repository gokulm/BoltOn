namespace BoltOn.Bootstrapping
{
    public interface IBootstrapperPreRegistrationTask
    {
		void Run(PreRegistrationTaskContext context);
    }
}
