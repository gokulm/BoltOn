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
			// arrange
			var bootstrapper = Bootstrapper.Instance;

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
				.Configure(c => c.Container = container)
				.BoltOn();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(3)]
		public void Container_CallContainerAfterBoltOn_ReturnsDefaultContainer()
		{
			// act 
			Bootstrapper
				.Instance
				.BoltOn();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(4)]
		public void Container_CallContainerAfterBoltOnWithSetContainer_ReturnsNetStandardContainer()
		{
			// arrange 
			Bootstrapper
				.Instance
				.Configure(c => c.Container = new NetStandardContainerAdapter());

			// act 
			Bootstrapper
				.Instance
				.BoltOn();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
			Assert.IsType<NetStandardContainerAdapter>(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(4)]
		public void BoltOn_ExcludeAllDIAssemblies_ThrowsException()
		{
			// arrange & act 
			var ex = Record.Exception(() =>
			{
				Bootstrapper
					.Instance
					.Configure(b =>
					{
						b.AssemblyOptions = new BoltOnIoCAssemblyOptions
						{
							AssembliesToBeExcluded = new List<System.Reflection.Assembly>
							{
								typeof(NetStandardContainerAdapter).Assembly,
								typeof(SimpleInjectorContainerAdapter).Assembly
							}
						};
					})
					.BoltOn();
			});

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
				.Configure(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
							{
								typeof(NetStandardContainerAdapter).Assembly
							}
					};
				});

			// act
			Bootstrapper
				.Instance
				.BoltOn();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
			Assert.IsType<SimpleInjectorContainerAdapter>(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(6)]
		public void BoltOn_ExcludeAssemblyWithRegistrationTask_ThrowsException()
		{
			// arrange
			Bootstrapper
			.Instance
			.Configure(b =>
			{
				b.AssemblyOptions = new BoltOnIoCAssemblyOptions
				{
					AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
					typeof(BootstrapperTests).Assembly,
						}
				};
			});

			Bootstrapper
				.Instance
				.BoltOn();

			// act 
			// as this could throw any exception specific to the DI framework, using record
			var ex = Record.Exception(() => ServiceLocator.Current.GetInstance<ITestService>());

			// assert
			Assert.NotNull(ex);
		}

		[Fact, TestPriority(7)]
		public void BoltOn_ConcreteClassWithoutRegistrationButResolvableDependencies_ReturnsInstance()
		{
			// arrange
			Bootstrapper
				.Instance
				.Configure(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
							typeof(BootstrapperTests).Assembly,
						}
					};
				})
				.BoltOn();

			// act 
			// as this could throw any exception specific to the DI framework, using record
			var employee = ServiceLocator.Current.GetInstance<Employee>();

			// assert
			Assert.NotNull(employee);
		}

		[Fact, TestPriority(7)]
		public void BoltOn_ConcreteClassWithoutRegistrationButNotResolvableDependencies_ThrowsException()
		{
			// arrange
			Bootstrapper
				.Instance
				.Configure(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
							typeof(BootstrapperTests).Assembly,
						}
					};
				})
				.BoltOn();

			// act 
			// as this could throw any exception specific to the DI framework, using record
			var ex = Record.Exception(() => ServiceLocator.Current.GetInstance<ClassWithInjectedDependency>());

			// assert
			Assert.NotNull(ex);
		}

		[Fact, TestPriority(8)]
		public void BoltOn_DefaultBoltOnWithAllTheAssemblies_RunsRegistrationTasksAndResolvesDependencies()
		{
			// arrange
			Bootstrapper
				.Instance
				.BoltOn();

			// act
			var employee = ServiceLocator.Current.GetInstance<Employee>();

			// assert
			var name = employee.GetName();
			Assert.Equal("John", name);
		}

		[Fact, TestPriority(9)]
		public void BoltOn_DefaultBoltOnWithAllTheAssemblies_ResolvesDependenciesRegisteredByConvention()
		{
			// arrange
			Bootstrapper
				.Instance
				.BoltOn();

			// act
			var result = ServiceLocator.Current.GetInstance<ITestService>();

			// assert
			var name = result.GetName();
			Assert.Equal("test", name);
		}

		[Fact]
		public void BoltOn_ClassNotRegisteredByConvention_ThrowsException()
		{
			// arrange
			Bootstrapper
				.Instance
				.Configure(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
							typeof(BootstrapperTests).Assembly,
						}
					};
				})
				.BoltOn();

			// act 
			// as this could throw any exception specific to the DI framework, using record
			var ex = Record.Exception(() => ServiceLocator.Current.GetInstance<ITestService>());

			// assert
			Assert.NotNull(ex);
		}

		[Fact]
		public void BoltOn_BoltOnCalledMoreThanOnce_ThrowsException()
		{
			// arrange
			Bootstrapper
				.Instance
				.BoltOn();

			// act and assert
			Assert.Throws<Exception>(() => Bootstrapper.Instance.BoltOn());
		}

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
