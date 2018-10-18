namespace BoltOn.Bootstrapping
{
	public interface IBootstrapperRegistrationTask
	{
		void Run(RegistrationTaskContext context);
	}
}
