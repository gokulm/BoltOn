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
		private List<Assembly> _assemblies;
		private bool _isDisposed;
		private Assembly _callingAssembly;
		private HashSet<Assembly> _assembliesToBeExcluded;

		private Bootstrapper()
		{
			_assemblies = new List<Assembly>();
			_assembliesToBeExcluded = new HashSet<Assembly>();
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

		public Bootstrapper SetContainer(IBoltOnContainer boltOnContainer)
		{
			_container = boltOnContainer;
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
			// todo (medium): should allow custom app prefix. to accomodate solutions with different project names
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
				task.Run(_container, _assemblies);
			}

			_container.RegisterSingleton(typeof(IoC.IServiceProvider), new ServiceProvider(_container));

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

			//foreach (var type in registrationTaskTypes)
			//{
			//	_container.RegisterTransient(registrationTaskType, type);
			//}
			_container.RegisterTransientCollection(registrationTaskType, registrationTaskTypes);
		}

		private void RunPostRegistrationTasks()
		{
			var postRegistrationTasks = _container.GetAllInstances<IBootstrapperPostRegistrationTask>();
			if (postRegistrationTasks != null)
			{
				postRegistrationTasks.ToList().ForEach(t => t.Run());
			}
		}

		private void CreateContainer()
		{
			if(_container == null)
			{
				var containerInterfaceType = typeof(IBoltOnContainer);
				var containerType = (from a in _assemblies
											 from t in a.GetTypes()
											 where containerInterfaceType.IsAssignableFrom(t)
											 && t.IsClass
				                            select t).LastOrDefault();
				if (containerType == null)
					throw new Exception("No IoC Container Adapter referenced");

				_container = Activator.CreateInstance(containerType) as IBoltOnContainer;
			}

			ServiceLocator.SetContainer(_container);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_container != null)
				{
					// this can be used to clear all the managed and unmanaged resources
					if (!_isDisposed)
					{
						_container.Dispose();
						_isDisposed = true;
					}
					_container = null;
				}
				_assemblies.Clear();
				_assembliesToBeExcluded.Clear();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
