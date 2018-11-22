using System;
using BoltOn.Bootstrapping;
using BoltOn.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
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
			var serviceCollection = new ServiceCollection();

			// act 
			serviceCollection.BoltOn();

			// assert
			Assert.NotNull(Bootstrapper.Instance.Container);
		}

		[Fact, TestPriority(3)]
		public void BoltOn_ExcludeAssemblyWithRegistrationTask_ThrowsException()
		{
			// arrange	
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(options =>
			{
				options.ExcludeAssemblies(typeof(ITestService).Assembly);
			});
			var serviceProvider = serviceCollection.BuildServiceProvider();

			// act 
			var ex = Record.Exception(() => serviceProvider.GetRequiredService<ITestService>());

			// assert
			Assert.NotNull(ex);
		}

		[Fact, TestPriority(4)]
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

		[Fact, TestPriority(5)]
		public void BoltOn_ConcreteClassWithoutRegistrationButNotResolvableDependencies_ThrowsException()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.BoltOn(options =>
			{
				options.ExcludeAssemblies(typeof(ITestService).Assembly);
			});
			var serviceProvider = serviceCollection.BuildServiceProvider();

			// act 
			var instance = serviceProvider.GetService<ClassWithInjectedDependency>();
			var ex = Record.Exception(() => serviceProvider.GetRequiredService<ClassWithInjectedDependency>());

			// assert
			Assert.Null(instance);
			Assert.NotNull(ex);
		}

		//[Fact, TestPriority(8)]
		//public void BoltOn_DefaultBoltOnWithAllTheAssemblies_RunsRegistrationTasksAndResolvesDependencies()
		//{
		//	// arrange
		//	Bootstrapper
		//		.Instance
		//		.ConfigureIoC(b =>
		//		{
		//			//b.AssemblyOptions = new BoltOnIoCAssemblyOptions
		//			//{
		//			//	AssembliesToBeExcluded = new List<System.Reflection.Assembly>
		//			//	{
		//			//	typeof(SimpleInjectorContainerAdapter).Assembly,
		//			//	}
		//			//};
		//		})
		//		.BoltOn();

		//	// act
		//	var employee = ServiceLocator.Current.GetInstance<Employee>();

		//	// assert
		//	var name = employee.GetName();
		//	Assert.Equal("John", name);
		//}

		//[Fact, TestPriority(9)]
		//public void BoltOn_DefaultBoltOnWithAllTheAssemblies_ResolvesDependenciesRegisteredByConvention()
		//{
		//	// arrange
		//	Bootstrapper
		//		.Instance
		//		.ConfigureIoC(b =>
		//		{
		//			//b.AssemblyOptions = new BoltOnIoCAssemblyOptions
		//			//{
		//			//	AssembliesToBeExcluded = new List<System.Reflection.Assembly>
		//			//	{
		//			//	typeof(SimpleInjectorContainerAdapter).Assembly,
		//			//	}
		//			//};
		//		})
		//		.BoltOn();

		//	// act
		//	var result = ServiceLocator.Current.GetInstance<ITestService>();

		//	// assert
		//	var name = result.GetName();
		//	Assert.Equal("test", name);
		//}

		//[Fact, TestPriority(10)]
		//public void BoltOn_ClassNotRegisteredByConventionUsingNetStandardContainer_ReturnsNull()
		//{
		//	// arrange
		//	Bootstrapper
		//		.Instance
		//		.ConfigureIoC(b =>
		//		{
		//			//b.AssemblyOptions = new BoltOnIoCAssemblyOptions
		//			//{
		//			//	AssembliesToBeExcluded = new List<System.Reflection.Assembly>
		//			//	{
		//			//		typeof(BootstrapperTests).Assembly,
		//			//		typeof(SimpleInjectorContainerAdapter).Assembly,
		//			//	}
		//			//};
		//		})
		//		.BoltOn();

		//	// act 
		//	var instance = ServiceLocator.Current.GetInstance<ITestService>();

		//	// assert
		//	Assert.Null(instance);
		//}

		//[Fact, TestPriority(11)]
		//public void BoltOn_ClassNotRegisteredByConventionUsingSimpleInjector_ThrowsException()
		//{
		//	// arrange

		//	var container = new Container();
		//	container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
		//	container.Options.AllowOverridingRegistrations = true;
		//	container.Options.ConstructorResolutionBehavior = new FewParameterizedConstructorBehavior();

		//	// act 
		//	using (AsyncScopedLifestyle.BeginScope(container))
		//	{
		//		Bootstrapper
		//		.Instance
		//		.ConfigureIoC(b =>
		//		{
		//			//b.AssemblyOptions = new BoltOnIoCAssemblyOptions
		//			//{
		//			//	AssembliesToBeExcluded = new List<System.Reflection.Assembly>
		//			//	{
		//			//		typeof(BootstrapperTests).Assembly,
		//			//		typeof(NetStandardContainerAdapter).Assembly,
		//			//	}
		//			//};
		//			b.Container = new SimpleInjectorContainerAdapter(container);
		//		})
		//		.BoltOn();

		//		// as this could throw any exception specific to the DI framework, using record
		//		var ex = Record.Exception(() => ServiceLocator.Current.GetInstance<ITestService>());

		//		// assert
		//		Assert.NotNull(ex);
		//	}
		//}

		//[Fact, TestPriority(12)]
		//public void BoltOn_BoltOnCalledMoreThanOnce_ThrowsException()
		//{
		//	// arrange
		//	Bootstrapper
		//		.Instance
		//		.ConfigureIoC(b =>
		//		{
		//			//b.AssemblyOptions = new BoltOnIoCAssemblyOptions
		//			//{
		//			//	AssembliesToBeExcluded = new List<System.Reflection.Assembly>
		//			//	{
		//			//		typeof(SimpleInjectorContainerAdapter).Assembly,
		//			//	}
		//			//};
		//		})
		//		.BoltOn();

		//	// act and assert
		//	Assert.Throws<Exception>(() => Bootstrapper.Instance.BoltOn());
		//}

		//public void BoltOnIoC_ConfigureIoC_BootstrapsApp()
		//{
		//	ServiceCollection serviceCollection = new ServiceCollection();
		//	serviceCollection.BoltOnIoC(options =>
		//	{
		//		options.AssemblyOptions = new BoltOnIoCAssemblyOptions
		//		{
		//			AssembliesToBeExcluded = new List<System.Reflection.Assembly>
		//				{
		//					typeof(SimpleInjectorContainerAdapter).Assembly,
		//				}
		//		};
		//	});
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
