using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Other
{
	public class TestMediatorDataRegistrationTask : IRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            var changeTrackerInterceptor = new Mock<IBoltOnLogger<ChangeTrackerInterceptor>>();
            changeTrackerInterceptor.Setup(s => s.Debug(It.IsAny<string>()))
                                     .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
            context.Container.AddTransient(s => changeTrackerInterceptor.Object);
        }
    }
}
