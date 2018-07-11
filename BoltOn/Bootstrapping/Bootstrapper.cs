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
		private Assembly _callingAssembly;
		private HashSet<Assembly> _assembliesToBeExcluded = new HashSet<Assembly>();

		private Bootstrapper()
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

		/// <summary>
		/// This method can be used if a container needs to be created with customization specific to 
		/// the DI framework/library
		/// </summary>
		public Bootstrapper CreateContainer<TContainer>(TContainer container) 
			where TContainer : IBoltOnContainer
		{
			_container = container;
			ServiceLocator.SetContainer(_container);
			return this;
		}

		public Bootstrapper ExcludeAssemblies(params Assembly[] assemblies)
		{
			assemblies.ToList().ForEach(a =>
			{
				_assembliesToBeExcluded.Add(a);
			});
			return this;
		}

		private void PopulateAssembliesByConvention()
		{
			var appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var referencedAssemblies = _callingAssembly.GetReferencedAssemblies().ToList();
			referencedAssemblies.Add(_callingAssembly.GetName());
			// get BoltOn assemblies
			var boltOnAssemblies = GetAssemblies("BoltOn");
			boltOnAssemblies
				.OrderBy(o => o.Item2)
				.ToList()
				.ForEach(f => _assemblies.Add(f.Item1));
			// get app assemblies
			var appPrefix = _callingAssembly.GetName().Name.Split('.')[0];
			var appAssemblies = GetAssemblies(appPrefix);
			appAssemblies
				.OrderBy(o => o.Item2)
				.ToList()
				.ForEach(f => _assemblies.Add(f.Item1));
			var assembliesToBeExcludedNames = _assembliesToBeExcluded.Select(s => s.FullName).ToList();
			_assemblies = _assemblies.Where(a => !assembliesToBeExcludedNames.Contains(a.FullName)).Distinct().ToList();

			List<Tuple<Assembly, int>> GetAssemblies(string startsWith)
			{
				var temp = (from r in referencedAssemblies
							join a in appDomainAssemblies
							on r.FullName equals a.FullName
            				where r.Name.StartsWith(startsWith, StringComparison.Ordinal)
							let orderAttribute = a.GetCustomAttribute<AssemblyRegistrationOrderAttribute>()
	                        select new Tuple<Assembly, int>
	                        (a, orderAttribute != null ? orderAttribute.Order : int.MaxValue)).Distinct().ToList();
				return temp;
			}
		}

		public void Run()
		{
			_callingAssembly = Assembly.GetCallingAssembly();
			PopulateAssembliesByConvention();
			CreateContainer();
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
			                                && t.IsClass
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
			                             && t.IsClass
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
			                             && t.IsClass
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

		private void CreateContainer()
		{
			if(_container == null)
			{
				var containerFactoryInterfaceType = typeof(IBoltOnContainerFactory);
				// only one container factory will be if there are many DI libraries included
				var containerFactoryType = (from a in _assemblies
											 from t in a.GetTypes()
											 where containerFactoryInterfaceType.IsAssignableFrom(t)
											 && t.IsClass
				                            select t).First();
				var task = Activator.CreateInstance(containerFactoryType) as IBoltOnContainerFactory;
				_container = task.Create();
			}
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
