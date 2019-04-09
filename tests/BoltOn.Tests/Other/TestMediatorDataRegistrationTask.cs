using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Other
{
	public class TestMediatorDataRegistrationTask : IRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            var mediatorContextBuilderInterceptor = new Mock<IBoltOnLogger<MediatorContextInterceptor>>();
            mediatorContextBuilderInterceptor.Setup(s => s.Debug(It.IsAny<string>()))
                                     .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
            context.Container.AddTransient(s => mediatorContextBuilderInterceptor.Object);
        }
    }
}
