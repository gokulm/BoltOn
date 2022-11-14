using System;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Transaction;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Requestor.Fakes
{
    public static class RequestorRegistrationTask
    {
        public static void RegisterRequestorFakes(this BootstrapperOptions bootstrapperOptions)
        {
            bootstrapperOptions.ServiceCollection.AddLogging();
            var appClock = new Mock<IAppClock>();
            var currentDateTime = DateTime.Parse("10/27/2018 12:51:59 PM");
            appClock.Setup(s => s.Now).Returns(currentDateTime);
            bootstrapperOptions.ServiceCollection.AddTransient((s) => appClock.Object);
        }
    }
}
