using BoltOn.Other;

namespace BoltOn.Tests.Bootstrapping.Fakes
{
	public interface ITestExcludeRegistrationService
	{
		string GetName();
	}

	[ExcludeFromRegistration]
    public class TestExcludeRegistrationService : ITestExcludeRegistrationService
    {
        public string GetName()
        {
            return "test";
        }
    }
}
