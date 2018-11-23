namespace BoltOn.Bootstrapping
{
    public interface IBootstrapperPreRegistrationTask
    {
		void Run(PreRegistrationTaskContext context);
    }

	public interface IBootstrapperRegistrationTask
	{
		void Run(RegistrationTaskContext context);
	}

	public interface IBootstrapperPostRegistrationTask
	{
		void Run(PostRegistrationTaskContext context);
	}
}
