namespace BoltOn.Bootstrapping
{
	public interface IBootstrapperRegistrationTask
	{
		void Run(RegistrationTaskContext context);
	}

	public interface IBootstrapperPostRegistrationTask
	{
		void Run(PostRegistrationTaskContext context);
	}
}
