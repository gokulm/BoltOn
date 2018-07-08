using System;
using BoltOn.IoC;
using BoltOn.IoC.NetStandardBolt;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace BoltOn.Tests.IoC
{
	public class NetStandardContainerAdapterTests : IDisposable
    {
		private IBoltOnContainer _container;

		[Fact]
		public void GetInstance_RegisterTransient_ResolvesDependencies()
		{
			_container = new NetStandardContainerAdapter();
			ServiceLocator.SetContainer(_container);
			_container.RegisterTransient<ITestService, TestService>();
			_container.LockRegistration();

			// act
			var service1 = _container.GetInstance<ITestService>();
			service1.SetName("abc");
			var service2 = _container.GetInstance<ITestService>();
			var name = service2.GetName();

			// assert
			Assert.Equal("Test", name);
		}

		[Fact]
		public void GetInstance_RegisterSingleton_ResolvesDependencies()
		{
			// arrange
			_container = new NetStandardContainerAdapter();
			ServiceLocator.SetContainer(_container);
			_container.RegisterSingleton<ITestService, TestService>();
			_container.LockRegistration();

			// act
			var service1 = _container.GetInstance<ITestService>();
			service1.SetName("abc");
			var service2 = _container.GetInstance<ITestService>();
			var name = service2.GetName();

			// assert
			Assert.Equal("abc", name);
		}

		[Fact]
		public void GetInstance_RegisterScoped_ResolvesDependencies()
		{
			// arrange
			_container = new NetStandardContainerAdapter();
			ServiceLocator.SetContainer(_container);
			_container.RegisterScoped<ITestService, TestService>();
			_container.LockRegistration();

			// act
			var service1 = _container.GetInstance<ITestService>();
			service1.SetName("abc");
			var service2 = _container.GetInstance<ITestService>();
			var name = service2.GetName();

			// assert
			Assert.Equal("abc", name);
		}

		[Fact]
		public void GetInstance_RegisterTransientAndInjectService_ResolvesDependencies()
		{
			// arrange
			_container = new NetStandardContainerAdapter();
			ServiceLocator.SetContainer(_container);
			var autoMocker = new AutoMocker();
			var employee = autoMocker.CreateInstance<Employee>();
			var testService = autoMocker.GetMock<ITestService>();
			testService.Setup(s => s.GetName()).Returns("Test");
			_container.RegisterTransient(() => employee);
			_container.LockRegistration();

			// act
			var resolvedEmployee = _container.GetInstance<Employee>();
			var name = resolvedEmployee.GetName();

			// assert
			Assert.Equal("Test", name);
			testService.Verify(v => v.GetName());
		}

		[Fact]
		public void ServiceLocatorGetInstance_RegisterTransient_ResolvesDependencies()
		{
			_container = new NetStandardContainerAdapter();
			ServiceLocator.SetContainer(_container);
			_container.RegisterTransient<ITestService, TestService>();
			_container.LockRegistration();

			// act
			var service1 = ServiceLocator.Current.GetInstance<ITestService>();
			service1.SetName("abc");
			var service2 = ServiceLocator.Current.GetInstance<ITestService>();
			var name = service2.GetName();

			// assert
			Assert.Equal("Test", name);
		}


		public void Dispose()
		{
			if (_container != null)
			{
				_container.Dispose();
				_container = null;
			}
		}

		private class Employee
		{
			private readonly ITestService _testService;

			public Employee(ITestService testService)
			{
				_testService = testService;
			}

			public virtual string GetName()
			{
				return _testService.GetName();
			}
		}

		public interface ITestService
		{
			string GetName();
			void SetName(string name);
		}

		public class TestService : ITestService
		{
			string _name;
			
			public string GetName()
			{
				return _name ?? "Test";
			}

			public void SetName(string name)
			{
				_name = name;
			}
		}
	}
}
