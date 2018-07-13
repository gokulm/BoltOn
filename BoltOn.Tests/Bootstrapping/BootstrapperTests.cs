﻿using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.IoC;
using BoltOn.IoC.NetStandardBolt;
using BoltOn.IoC.SimpleInjector;
using BoltOn.Logging;
using BoltOn.Logging.NetStandard;
using BoltOn.Tests.Common;
using Moq;
using SimpleInjector.Lifestyles;
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
		public void Container_CallContainerAfterInitializingFactory_ThrowsException()
		{
			// arrange
			var containerFactory = new NetStandardContainerFactory();

			// act 
			Bootstrapper
				.Instance
				.SetContainerFactory(containerFactory);

			// assert
			Assert.Throws<Exception>(() => Bootstrapper.Instance.Container);
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
		public void Container_CallContainerAfterRunWithSetContainerFactory_ReturnsNetStandardContainer()
		{
			// act 
			Bootstrapper
				.Instance
				.SetContainerFactory(new NetStandardContainerFactory())
				.Run();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
			Assert.IsType<NetStandardContainerAdapter>(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(4)]
		public void Run_IncludeAllReferencedAssemblies_RunsRegistrationTasks()
		{
			// arrange
			Bootstrapper
				.Instance
				//.CreateContainer(new NetStandardContainerAdapter())
				//.ExcludeAssemblies(typeof(NetStandardLoggerAdapter<>).Assembly)
				.Run();
			var employee = ServiceLocator.Current.GetInstance<Employee>();

			// act
			var name = employee.GetName();

			// assert
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
