using System;
using BoltOn.Bootstrapping;
using BoltOn.IoC;
using BoltOn.IoC.NetStandardBolt;
using BoltOn.IoC.SimpleInjector;
using BoltOn.Logging;
using BoltOn.Tests.Common;
using Moq;
using Xunit;

namespace BoltOn.Tests.Bootstrapping
{
	[TestCaseOrderer("BoltOn.Tests.Common.PriorityOrderer", "BoltOn.Tests")]
	public class BootstrapperTests : IDisposable
	{
		[Fact, TestPriority(1)]
		public void Container_CallContainerBeforeInitializingFactory_ThrowsException()
		{
			// act and assert
			Assert.Throws<Exception>(() => Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(2)]
		public void Container_CallContainerAfterInitializingContainer_ReturnsContainer()
		{
			// arrange
			var container = new NetStandardContainerAdapter();

			// act 
			Bootstrapper
				.Instance
				.SetContainer(container);

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(3)]
		public void Container_CallContainerAfterRun_ReturnsContainer()
		{
			// act 
			Bootstrapper
				.Instance
				.Run();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(4)]
		public void Container_CallContainerAfterRunWithSetContainer_ReturnsNetStandardContainer()
		{
			// arrange 
			Bootstrapper
				.Instance
				.SetContainer(new NetStandardContainerAdapter());

			// act 
			Bootstrapper
				.Instance
				.Run();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
			Assert.IsType<NetStandardContainerAdapter>(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(4)]
		public void Run_ExcludeAllDIAssemblies_ThrowsException()
		{
			// arrange 
			Bootstrapper
				.Instance
				.ExcludeAssemblies(typeof(NetStandardContainerAdapter).Assembly, 
				                   typeof(SimpleInjectorContainerAdapter).Assembly);

			// act 
			var ex = Record.Exception(() => Bootstrapper.Instance.Run());

			// assert
			Assert.NotNull(ex);
			Assert.Same("No IoC Container Adapter referenced", ex.Message);
		}

		[Fact, TestPriority(5)]
		public void Container_CallContainerExcludeNetStandard_ReturnsSimpleInjectorContainer()
		{
			// arrange 
			Bootstrapper
				.Instance
				// if other DI frameworks/libraries are added, they should be excluded too
				.ExcludeAssemblies(typeof(NetStandardContainerAdapter).Assembly);

			// act
			Bootstrapper
				.Instance
				.Run();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
			Assert.IsType<SimpleInjectorContainerAdapter>(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(6)]
		public void Run_ExcludeAssemblyWithRegistrationTask_ThrowsException()
		{
			// arrange
			Bootstrapper
				.Instance
				.ExcludeAssemblies(typeof(BootstrapperTests).Assembly)
				.Run();

			// act 
			// as this could throw any exception specific to the DI framework, using record
			var ex = Record.Exception(() => ServiceLocator.Current.GetInstance<Employee>());

			// assert
			Assert.NotNull(ex);
		}

		[Fact, TestPriority(7)]
		public void Run_DefaultRunWithAllTheAssemblies_RunsRegistrationTasksAndResolvesDependencies()
		{
			// arrange
			Bootstrapper
				.Instance
				.Run();

			// act
			var employee = ServiceLocator.Current.GetInstance<Employee>();

			// assert
			var name = employee.GetName();
			Assert.Equal("John", name);
		}

		public void Dispose()
		{
			Bootstrapper
				.Instance
				.Dispose();
		}
	}

	public class TestBootstrapperRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(IBoltOnContainer container)
		{
			var loggerMock = new Mock<IBoltOnLogger<Employee>>();
			container.RegisterTransient<Employee>()
					 .RegisterTransient(() => loggerMock.Object);
		}
	}

	public class Employee
	{
		readonly IBoltOnLogger<Employee> _logger;

		public Employee(IBoltOnLogger<Employee> logger)
		{
			_logger = logger;
			_logger.Info("Employee instantiated...");
		}

		public string GetName()
		{
			_logger.Debug("getting name...");
			return "John";
		}
	}
}
