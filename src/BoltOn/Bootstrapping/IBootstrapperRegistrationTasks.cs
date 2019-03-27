namespace BoltOn.Bootstrapping
{
	public interface IRegistrationTask
	{
		void Run(RegistrationTaskContext context);
	}

	public interface IPostRegistrationTask
	{
		void Run(PostRegistrationTaskContext context);
	}
}
