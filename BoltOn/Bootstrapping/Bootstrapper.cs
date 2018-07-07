using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.IoC;

namespace BoltOn.Bootstrapping
{
	public class Bootstrapper : IDisposable
	{
		private static readonly Lazy<Bootstrapper> _instance = new Lazy<Bootstrapper>(() => new Bootstrapper());
		private IBoltOnContainer _container;
		private List<Assembly> _assemblies = new List<Assembly>();
		private bool _isDisposed;

		Bootstrapper()
		{
		}

		public static Bootstrapper Instance => _instance.Value;

		internal IBoltOnContainer Container
		{
			get
			{
				if (_container == null)
				{
					throw new Exception("Container not created");
				}
				return _container;
			}
		}

		public Bootstrapper CreateContainer<TContainer>() where TContainer : IBoltOnContainer, new()
		{
			_container = new TContainer();
			ServiceLocator.SetContainer(_container);
			return this;
		}

		public Bootstrapper ForAssemblies(Assembly[] assemblies)
		{
			assemblies.ToList().ForEach(a =>
			{
				_assemblies.Add(a);
			});
			return this;
		}

		public void Run()
		{
			RunPreRegistrationTasks();
			RunRegistrationTasks();
			_container.LockRegistration();
			RunPostRegistrationTasks();
		}

		private void RunPreRegistrationTasks()
		{
			var preRegistrationTaskType = typeof(IBootstrapperPreRegistrationTask);
			var preRegistrationTaskTypes = (from a in _assemblies
											from t in a.GetTypes()
											where preRegistrationTaskType.IsAssignableFrom(t)
											select t).ToList();

			foreach (var type in preRegistrationTaskTypes)
			{
				var task = Activator.CreateInstance(type) as IBootstrapperPreRegistrationTask;
				task.Run();
			}
		}

		private void RunRegistrationTasks()
		{
			var registrationTaskType = typeof(IBootstrapperRegistrationTask);
			var registrationTaskTypes = (from a in _assemblies
										 from t in a.GetTypes()
										 where registrationTaskType.IsAssignableFrom(t)
										 select t).ToList();

			foreach (var type in registrationTaskTypes)
			{
				var task = Activator.CreateInstance(type) as IBootstrapperRegistrationTask;
				task.Run(_container);
			}

			RegisterPostRegistrationTasks();
		}

		private void RegisterPostRegistrationTasks()
		{
			var registrationTaskType = typeof(IBootstrapperPostRegistrationTask);
			var registrationTaskTypes = (from a in _assemblies
										 from t in a.GetTypes()
										 where registrationTaskType.IsAssignableFrom(t)
										 select t).ToList();

			foreach (var type in registrationTaskTypes)
			{
				_container.RegisterTransient(registrationTaskType, type);
			}
		}

		private void RunPostRegistrationTasks()
		{
			var postRegistrationTasks = _container.GetAllInstances<IBootstrapperPostRegistrationTask>().ToList();
			postRegistrationTasks.ForEach(t => t.Run());
		}


		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					if (_container != null)
					{
						_container.Dispose();
						_container = null;
					}
				}

				_isDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}

	public interface IBootstrapperPreRegistrationTask
	{
		void Run();
	}

	public interface IBootstrapperRegistrationTask
	{
		void Run(IBoltOnContainer container);
	}

	public interface IBootstrapperPostRegistrationTask
	{
		void Run();
	}
}
