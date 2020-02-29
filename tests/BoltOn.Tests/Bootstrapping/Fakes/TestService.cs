namespace BoltOn.Tests.Bootstrapping.Fakes
{
	public interface ITestService
	{
		string GetName();
	}

	public class TestService : ITestService
    {
        public string GetName()
        {
            return "test";
        }
    }
}
