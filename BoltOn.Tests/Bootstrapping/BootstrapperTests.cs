using System;
using System.Collections.Generic;
using BoltOn.Bootstrapping;
using BoltOn.IoC;
using BoltOn.IoC.NetStandardBolt;
using BoltOn.IoC.SimpleInjector;
using BoltOn.Tests.Common;
using Moq;
using SimpleInjector;
using SimpleInjector.Lifestyles;
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
				.ConfigureIoC(c => c.Container = container)
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
					.ConfigureIoC(b =>
					{
						b.AssemblyOptions = new BoltOnIoCAssemblyOptions
						{
							AssembliesToBeExcluded = new List<System.Reflection.Assembly>
							{
								typeof(SimpleInjectorContainerAdapter).Assembly
							}
						};
					})
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
				.ConfigureIoC(c => c.Container = new NetStandardContainerAdapter());

			// act 
			Bootstrapper
				.Instance
				.BoltOn();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
			Assert.IsType<NetStandardContainerAdapter>(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(5)]
		public void Container_CallContainerExcludeNetStandard_ReturnsSimpleInjectorContainer()
		{
			// arrange 
			var container = new Container();
			container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
			container.Options.AllowOverridingRegistrations = true;
			container.Options.ConstructorResolutionBehavior = new FewParameterizedConstructorBehavior();
			Bootstrapper
				.Instance
				.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
							{
								typeof(NetStandardContainerAdapter).Assembly
							}
					};
					b.Container = new SimpleInjectorContainerAdapter(container);
				});

			// act
			using (AsyncScopedLifestyle.BeginScope(container))
			{
				Bootstrapper
					.Instance
					.BoltOn();
			}

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
			Assert.IsType<SimpleInjectorContainerAdapter>(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(4)]
		public void BoltOn_ExcludeAllDIAssemblies_ThrowsException()
		{
			// arrange & act 
			var ex = Record.Exception(() =>
			{
				Bootstrapper
					.Instance
					.ConfigureIoC(b =>
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

		[Fact, TestPriority(6)]
		public void BoltOn_ExcludeAssemblyWithRegistrationTask_ThrowsException()
		{
			// arrange	
			var container = new Container();
			container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
			container.Options.AllowOverridingRegistrations = true;
			container.Options.ConstructorResolutionBehavior = new FewParameterizedConstructorBehavior();
			Bootstrapper
			.Instance
			.ConfigureIoC(b =>
			{
				b.AssemblyOptions = new BoltOnIoCAssemblyOptions
				{
					AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
					typeof(BootstrapperTests).Assembly,
						typeof(NetStandardContainerAdapter).Assembly
						}
				};
				b.Container = new SimpleInjectorContainerAdapter(container);
			});

			using (AsyncScopedLifestyle.BeginScope(container))
			{
				Bootstrapper
					.Instance
					.BoltOn();


				// act 
				// as this could throw any exception specific to the DI framework, using record
				var ex = Record.Exception(() => ServiceLocator.Current.GetInstance<ITestService>());

				// assert
				Assert.NotNull(ex);
			}
		}

		[Fact, TestPriority(7)]
		public void BoltOn_ConcreteClassWithoutRegistrationButResolvableDependencies_ReturnsInstance()
		{
			// arrange
			Bootstrapper
				.Instance
				.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
								typeof(SimpleInjectorContainerAdapter).Assembly
						}
					};
				})
				.BoltOn();

			// act 
			var employee = ServiceLocator.Current.GetInstance<Employee>();

			// assert
			Assert.NotNull(employee);
		}

		[Fact, TestPriority(7)]
		public void BoltOn_ConcreteClassWithoutRegistrationButNotResolvableDependenciesNetStandardContainer_ThrowsException()
		{
			// arrange
			Bootstrapper
				.Instance
				.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
						typeof(BootstrapperTests).Assembly,
						typeof(SimpleInjectorContainerAdapter).Assembly,
						}
					};
				})
				.BoltOn();

			// act 
			var instance = ServiceLocator.Current.GetInstance<ClassWithInjectedDependency>();

			// assert
			Assert.Null(instance);
		}

		[Fact, TestPriority(8)]
		public void BoltOn_DefaultBoltOnWithAllTheAssemblies_RunsRegistrationTasksAndResolvesDependencies()
		{
			// arrange
			Bootstrapper
				.Instance
				.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
						typeof(SimpleInjectorContainerAdapter).Assembly,
						}
					};
				})
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
				.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
						typeof(SimpleInjectorContainerAdapter).Assembly,
						}
					};
				})
				.BoltOn();

			// act
			var result = ServiceLocator.Current.GetInstance<ITestService>();

			// assert
			var name = result.GetName();
			Assert.Equal("test", name);
		}

		[Fact, TestPriority(10)]
		public void BoltOn_ClassNotRegisteredByConventionUsingNetStandardContainer_ReturnsNull()
		{
			// arrange
			Bootstrapper
				.Instance
				.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
							typeof(BootstrapperTests).Assembly,
							typeof(SimpleInjectorContainerAdapter).Assembly,
						}
					};
				})
				.BoltOn();

			// act 
			var instance = ServiceLocator.Current.GetInstance<ITestService>();

			// assert
			Assert.Null(instance);
		}

		[Fact, TestPriority(11)]
		public void BoltOn_ClassNotRegisteredByConventionUsingSimpleInjector_ThrowsException()
		{
			// arrange

			var container = new Container();
			container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
			container.Options.AllowOverridingRegistrations = true;
			container.Options.ConstructorResolutionBehavior = new FewParameterizedConstructorBehavior();

			// act 
			using (AsyncScopedLifestyle.BeginScope(container))
			{
				Bootstrapper
				.Instance
				.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
							typeof(BootstrapperTests).Assembly,
							typeof(NetStandardContainerAdapter).Assembly,
						}
					};
					b.Container = new SimpleInjectorContainerAdapter(container);
				})
				.BoltOn();

				// as this could throw any exception specific to the DI framework, using record
				var ex = Record.Exception(() => ServiceLocator.Current.GetInstance<ITestService>());

				// assert
				Assert.NotNull(ex);
			}
		}

		[Fact, TestPriority(12)]
		public void BoltOn_BoltOnCalledMoreThanOnce_ThrowsException()
		{
			// arrange
			Bootstrapper
				.Instance
				.ConfigureIoC(b =>
				{
					b.AssemblyOptions = new BoltOnIoCAssemblyOptions
					{
						AssembliesToBeExcluded = new List<System.Reflection.Assembly>
						{
							typeof(SimpleInjectorContainerAdapter).Assembly,
						}
					};
				})
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
