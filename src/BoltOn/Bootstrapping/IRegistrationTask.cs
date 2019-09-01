namespace BoltOn.Bootstrapping
{
	public interface IRegistrationTask
	{
		void Run(RegistrationTaskContext context);
	}

	public interface ICleanupTask
	{
		void Run();
	}
}
