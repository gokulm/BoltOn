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

    [ExcludeFromRegistration]
    public interface ITestExcludeRegistrationService2
    {
        string GetName();
    }

    public class TestExcludeRegistrationService2 : ITestExcludeRegistrationService2
    {
        public string GetName()
        {
            return "test";
        }
    }
}
