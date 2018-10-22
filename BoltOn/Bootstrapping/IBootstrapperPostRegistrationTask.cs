namespace BoltOn.Bootstrapping
{
    public interface IBootstrapperPostRegistrationTask
    {
		void Run(RegistrationTaskContext context);
    }
}
