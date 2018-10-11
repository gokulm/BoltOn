using System;
using System.Collections.Generic;
using BoltOn.Bootstrapping;
using BoltOn.IoC;
using BoltOn.IoC.NetStandardBolt;
using BoltOn.IoC.SimpleInjector;
using BoltOn.Tests.Common;
using Moq;
using Xunit;

namespace BoltOn.Tests.Bootstrapping
{
	[TestCaseOrderer("BoltOn.Tests.Common.PriorityOrderer", "BoltOn.Tests")]
	public class BootstrapperTests : IDisposable
	{
		[Fact, TestPriority(1)]
		public void Container_CallContainerBeforeInitializingContainer_ThrowsException()
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
				.BoltOn();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
		}

		//[Fact, TestPriority(4)]
		//public void Container_CallContainerAfterRunWithSetContainer_ReturnsNetStandardContainer()
		//{
		//	// arrange 
		//	Bootstrapper
		//		.Instance
		//		.SetContainer(new NetStandardContainerAdapter());

		//	// act 
		//	Bootstrapper
		//		.Instance
		//		.BoltOn();

		//	// assert
		//	Assert.NotNull(Bootstrapper.Instance.Container);
		//	Assert.IsType<NetStandardContainerAdapter>(Bootstrapper.Instance.Container);
		//}

		[Fact, TestPriority(4)]
		public void Run_ExcludeAllDIAssemblies_ThrowsException()
		{
			// arrange 
			//Bootstrapper
			//.Instance
			//.ExcludeAssemblies(typeof(NetStandardContainerAdapter).Assembly,
			//typeof(SimpleInjectorContainerAdapter).Assembly);

			// arrange & act 
			var ex = Record.Exception(() =>
			{
				Bootstrapper
					.Instance
					.BoltOnSimpleInjector(b =>
					{
						b.AssemblyOptions = new BoltOnIoCAssemblyOptions
						{
							AssembliesToBeExcluded = new List<System.Reflection.Assembly>
							{
								typeof(NetStandardContainerAdapter).Assembly,
								typeof(SimpleInjectorContainerAdapter).Assembly
							}
						};
					});
			});

			// assert
			Assert.NotNull(ex);
			Assert.Same("No IoC Container Adapter referenced", ex.Message);
		}

		//[Fact, TestPriority(5)]
		//public void Container_CallContainerExcludeNetStandard_ReturnsSimpleInjectorContainer()
		//{
		//	// arrange 
		//	Bootstrapper
		//		.Instance
		//		// if other DI frameworks/libraries are added, they should be excluded too
		//		.ExcludeAssemblies(typeof(NetStandardContainerAdapter).Assembly);

		//	// act
		//	Bootstrapper
		//		.Instance
		//		.BoltOn();

		//	// assert
		//	Assert.NotNull(Bootstrapper.Instance.Container);
		//	Assert.IsType<SimpleInjectorContainerAdapter>(Bootstrapper.Instance.Container);
		//}

		//[Fact, TestPriority(6)]
		//public void Run_ExcludeAssemblyWithRegistrationTask_ThrowsException()
		//{
		//	// arrange
		//	//Bootstrapper
		//	//.Instance
		//	//.ExcludeAssemblies(typeof(BootstrapperTests).Assembly)
		//	//.Run();

		//	Bootstrapper
		//		.Instance
		//		//.Configure(o =>
		//		//{
		//		//	o.AssembliesToBeExcluded.Add(typeof(BootstrapperTests).Assembly);
		//		//})
		//		.BoltOn(b => b.AssembliesToBeExcluded.Add(typeof(BootstrapperTests).Assembly));

		//	// act 
		//	// as this could throw any exception specific to the DI framework, using record
		//	var ex = Record.Exception(() => ServiceLocator.Current.GetInstance<ITestService>());

		//	// assert
		//	Assert.NotNull(ex);
		//}

		//[Fact, TestPriority(7)]
		//public void Run_ConcreteClassWithoutRegistrationButResolvableDependencies_ReturnsInstance()
		//{
		//	// arrange
		//	Bootstrapper
		//		.Instance
		//		.ExcludeAssemblies(typeof(BootstrapperTests).Assembly)
		//		.BoltOn();

		//	// act 
		//	// as this could throw any exception specific to the DI framework, using record
		//	var employee = ServiceLocator.Current.GetInstance<Employee>();

		//	// assert
		//	Assert.NotNull(employee);
		//}

		//[Fact, TestPriority(7)]
		//public void Run_ConcreteClassWithoutRegistrationButNotResolvableDependencies_ThrowsException()
		//{
		//	// arrange
		//	Bootstrapper
		//		.Instance
		//		//.ExcludeAssemblies(typeof(BootstrapperTests).Assembly)
		//		.BoltOn(b => b.AssembliesToBeExcluded.Add(typeof(BootstrapperTests).Assembly));

		//	// act 
		//	// as this could throw any exception specific to the DI framework, using record
		//	var ex = Record.Exception(() => ServiceLocator.Current.GetInstance<ClassWithInjectedDependency>());

		//	// assert
		//	Assert.NotNull(ex);
		//}

		//[Fact, TestPriority(8)]
		//public void Run_DefaultRunWithAllTheAssemblies_RunsRegistrationTasksAndResolvesDependencies()
		//{
		//	// arrange
		//	Bootstrapper
		//		.Instance
		//		.BoltOn();

		//	// act
		//	var employee = ServiceLocator.Current.GetInstance<Employee>();

		//	// assert
		//	var name = employee.GetName();
		//	Assert.Equal("John", name);
		//}

		//[Fact, TestPriority(9)]
		//public void Run_DefaultRunWithAllTheAssemblies_ResolvesDependenciesRegisteredByConvention()
		//{
		//	// arrange
		//	Bootstrapper
		//		.Instance
		//		.BoltOn();

		//	// act
		//	var result = ServiceLocator.Current.GetInstance<ITestService>();

		//	// assert
		//	var name = result.GetName();
		//	Assert.Equal("test", name);
		//}

		//[Fact]
		//public void Run_ClassNotRegisteredByConvention_ThrowsException()
		//{
		//	// arrange
		//	Bootstrapper
		//		.Instance
		//		//.ExcludeAssemblies(typeof(BootstrapperTests).Assembly)
		//		.BoltOn(b => b.AssembliesToBeExcluded.Add(typeof(BootstrapperTests).Assembly));

		//	// act 
		//	// as this could throw any exception specific to the DI framework, using record
		//	var ex = Record.Exception(() => ServiceLocator.Current.GetInstance<ITestService>());

		//	// assert
		//	Assert.NotNull(ex);
		//}

		public void Dispose()
		{
			Bootstrapper
				.Instance
				.Dispose();
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
}
