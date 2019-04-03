using BoltOn.Bootstrapping;
using BoltOn.Data.EF.Mediator;
using BoltOn.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Other
{
	public class TestMediatorDataRegistrationTask : IRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            var efAutoDetectChangesInterceptor = new Mock<IBoltOnLogger<EFQueryTrackingBehaviorInterceptor>>();
            efAutoDetectChangesInterceptor.Setup(s => s.Debug(It.IsAny<string>()))
                                     .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
            context.Container.AddTransient((s) => efAutoDetectChangesInterceptor.Object);
        }
    }
}
