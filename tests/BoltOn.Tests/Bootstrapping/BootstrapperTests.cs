using System;
using System.Linq;
using BoltOn.Tests.Bootstrapping.Fakes;
using BoltOn.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Bootstrapping
{
	[TestCaseOrderer("BoltOn.Tests.Common.PriorityOrderer", "BoltOn.Tests")]
    [Collection("IntegrationTests")]
    public class BootstrapperTests : IDisposable
    {
        [Fact, TestPriority(6)]
        public void BoltOn_ConcreteClassWithoutRegistrationButResolvableDependencies_ReturnsInstance()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.BoltOn();
            serviceCollection.AddTransient<Employee>();
            serviceCollection.AddTransient<ClassWithInjectedDependency>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // act 
            var employee = serviceProvider.GetRequiredService<Employee>();

            // assert
            Assert.NotNull(employee);
        }

        [Fact, TestPriority(8)]
        public void BoltOn_DefaultBoltOnWithAllTheAssemblies_RunsRegistrationTasksAndResolvesDependencies()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.BoltOn();
            serviceCollection.AddTransient<Employee>();
            serviceCollection.AddTransient<ClassWithInjectedDependency>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // act 
            var employee = serviceProvider.GetRequiredService<Employee>();

            // assert
            var name = employee.GetName();
            Assert.Equal("John", name);
        }

        [Fact, TestPriority(9)]
        public void BoltOn_DefaultBoltOnWithAllTheAssemblies_ResolvesDependenciesRegisteredByConvention()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.BoltOn();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // act 
            var result = serviceProvider.GetRequiredService<ITestService>();

            // assert
            var name = result.GetName();
            Assert.Equal("test", name);
        }

        [Fact, TestPriority(12)]
        public void BoltOn_BoltOn_DoesNotExecutePostRegistrationTask()
        {
            // arrange
            var serviceCollection = new ServiceCollection();

            // act 
            serviceCollection.BoltOn();

            // assert
            var postRegistrationTaskIndex = BootstrapperRegistrationTasksHelper.Tasks.IndexOf($"Executed {typeof(TestBootstrapperPostRegistrationTask).Name}");
            Assert.True(postRegistrationTaskIndex == -1);
        }

        [Fact, TestPriority(13)]
        public void BoltOn_BoltOnAndTightenBolts_ExecutesAllPostRegistrationTasksInOrder()
        {
            // arrange
            BootstrapperRegistrationTasksHelper.Tasks.Clear();
            var serviceCollection = new ServiceCollection();

            // act 
            serviceCollection.AddLogging();
            serviceCollection.BoltOn();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.TightenBolts();

            // assert
            var postRegistrationTaskIndex = BootstrapperRegistrationTasksHelper.Tasks.IndexOf($"Executed {typeof(TestBootstrapperPostRegistrationTask).Name}");
            Assert.True(postRegistrationTaskIndex != -1);
        }

        [Fact, TestPriority(14)]
        public void BoltOn_BoltOnAndTightenBoltsWithExcludedFromRegistration_ReturnsNull()
        {
            // arrange
            var serviceCollection = new ServiceCollection();

            // act 
            serviceCollection.AddLogging();
            serviceCollection.BoltOn();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.TightenBolts();
            var test = serviceProvider.GetService<ITestExcludeRegistrationService>();

            // assert
            Assert.Null(test);
        }

        [Fact, TestPriority(11)]
        public void BoltOn_TightenBoltsCalledMoreThanOnce_PostRegistrationTasksGetCalledOnce()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // act
            serviceProvider.TightenBolts();
            serviceProvider.TightenBolts();

            // assert
            var postRegistrationTaskCount = BootstrapperRegistrationTasksHelper.Tasks
                                    .Count(w => w == $"Executed {typeof(TestBootstrapperPostRegistrationTask).Name}");
            Assert.True(postRegistrationTaskCount == 1);
        }

        [Fact, TestPriority(12)]
        public void BoltOn_TightenBolts_PostRegistrationWithDependencyGetCalledOnce()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // act
            serviceProvider.TightenBolts();

            // assert
            var postRegistrationTaskCount = BootstrapperRegistrationTasksHelper.Tasks
                                    .Count(w => w == "Executed test service");
            Assert.True(postRegistrationTaskCount == 1);
        }

        public void Dispose()
        {
            BootstrapperRegistrationTasksHelper.Tasks.Clear();
        }
    }
}
