using System;
using System.Collections.Generic;
using System.Linq;
using BoltOn.Bootstrapping;
using BoltOn.Other;
using BoltOn.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Bootstrapping
{
	[TestCaseOrderer("BoltOn.Tests.Common.PriorityOrderer", "BoltOn.Tests")]
	[Collection("IntegrationTests")]
	public class BootstrapperTests : IDisposable
	{
		[Fact, TestPriority(1)]
		public void Container_CallContainerBeforeInitializingContainer_ThrowsException()
		{
			// arrange
			var bootstrapper = Bootstrapper.Instance;

			// act and assert
			Assert.Throws<Exception>(() => Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(2)]
		public void Container_CallContainerAfterInitializingContainer_ReturnsContainer()
		{
			// arrange
			var serviceCollection = new ServiceCollection();

			// act 
			serviceCollection.BoltOn();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(6)]
		public void BoltOn_ConcreteClassWithoutRegistrationButResolvableDependencies_ReturnsInstance()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
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

		[Fact, TestPriority(11)]
		public void BoltOn_BoltOnCalledMoreThanOnce_ThrowsException()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn();

			// act and assert
			Assert.Throws<Exception>(() => serviceCollection.BoltOn());
		}

		[Fact, TestPriority(12)]
		public void BoltOn_BoltOn_ExecutesRegistrationTasksInOrderAndNotPostRegistrationTask()
		{
			// arrange
			var serviceCollection = new ServiceCollection();

			// act 
			serviceCollection.BoltOn();

			// assert
			var registrationTaskIndex = BootstrapperRegistrationTaskTester.Tasks.IndexOf($"Executed {typeof(TestBootstrapperRegistrationTask).Name}");
			var postRegistrationTaskIndex = BootstrapperRegistrationTaskTester.Tasks.IndexOf($"Executed {typeof(TestBootstrapperPostRegistrationTask).Name}");
			Assert.True(registrationTaskIndex != -1);
			Assert.True(postRegistrationTaskIndex == -1);
		}

		[Fact, TestPriority(13)]
		public void BoltOn_BoltOnAndUseBoltOn_ExecutesAllRegistrationTasksInOrder()
		{
			// arrange
			BootstrapperRegistrationTaskTester.Tasks.Clear();
			var serviceCollection = new ServiceCollection();

			// act 
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();

			// assert
			var registrationTaskIndex = BootstrapperRegistrationTaskTester.Tasks.IndexOf($"Executed {typeof(TestBootstrapperRegistrationTask).Name}");
			var postRegistrationTaskIndex = BootstrapperRegistrationTaskTester.Tasks.IndexOf($"Executed {typeof(TestBootstrapperPostRegistrationTask).Name}");
			Assert.True(registrationTaskIndex != -1);
			Assert.True(postRegistrationTaskIndex != -1);
			Assert.True(registrationTaskIndex < postRegistrationTaskIndex);
		}

		[Fact, TestPriority(14)]
		public void BoltOn_BoltOnAndUseBoltOnWithExcludedFromRegistration_ReturnsNull()
		{
			// arrange
			var serviceCollection = new ServiceCollection();

			// act 
			serviceCollection.AddLogging();
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var test = serviceProvider.GetService<ITestExcludeRegistrationService>();

			// assert
			Assert.Null(test);
		}

		public void Dispose()
		{
			Bootstrapper
				.Instance
				.Dispose();
			BootstrapperRegistrationTaskTester.Tasks.Clear();
		}
	}

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

	public class ClassWithInjectedDependency
	{
		public ClassWithInjectedDependency(ITestService testService)
		{
			Name = testService.GetName();
		}

		public string Name
		{
			get;
			set;
		}
	}

	public class TestBootstrapperPostRegistrationTask : IBootstrapperPostRegistrationTask
	{
		public void Run(PostRegistrationTaskContext context)
		{
			BootstrapperRegistrationTaskTester.Tasks.Add($"Executed {this.GetType().Name}");
		}
	}

	public class TestBootstrapperRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			BootstrapperRegistrationTaskTester.Tasks.Add($"Executed {this.GetType().Name}");
			context.Container
				   .AddTransient<Employee>()
				   .AddTransient<ClassWithInjectedDependency>();
		}
	}

	public class BootstrapperRegistrationTaskTester
	{
		public static List<string> Tasks { get; set; } = new List<string>();
	}
}
