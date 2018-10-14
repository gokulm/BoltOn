using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using BoltOn.IoC;

namespace BoltOn.Bootstrapping
{
	public class Bootstrapper : IDisposable
	{
		private static readonly Lazy<Bootstrapper> _instance = new Lazy<Bootstrapper>(() => new Bootstrapper());
		private IBoltOnContainer _container;
		private bool _isDisposed;
		private Assembly _callingAssembly;
		private Hashtable _boltOnOptions;
		private bool _isBolted;

		private Bootstrapper()
		{
			_boltOnOptions = new Hashtable();
			Assemblies = new List<Assembly>().AsReadOnly();
			_boltOnOptions = new Hashtable();
		}

		public static Bootstrapper Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		internal IBoltOnContainer Container
		{
			get
			{
				if (_container == null)
					throw new Exception("Container not created");
				return _container;
			}
			set
			{
				_container = value;
			}
		}

		internal IReadOnlyCollection<Assembly> Assemblies { get; set; }

		internal TOptionType GetOptions<TOptionType>() where TOptionType : class, new()
		{
			var configValue = _boltOnOptions[typeof(TOptionType).Name];
			if (configValue == null)
			{
				var options = new TOptionType();
				AddOptions(options);
				return options;
			}

			return (TOptionType)Convert.ChangeType(configValue, typeof(TOptionType));
		}

		public void BoltOn()
		{
			Contract.Requires(!_isBolted, "Components are already bolted!");
			_callingAssembly = Assembly.GetCallingAssembly();
			LoadAssemblies();
			RunRegistrationTasks();
			_container.LockRegistration();
			RunPostRegistrationTasks();
		}

		public void AddOptions<TOptionType>(TOptionType options) where TOptionType : class
		{
			_boltOnOptions.Add(typeof(TOptionType).Name, options);
		}

		private void LoadAssemblies()
		{
			var boltOnIoCOptions = GetOptions<BoltOnIoCOptions>();
			var referencedAssemblyNames = _callingAssembly.GetReferencedAssemblies().ToList();
			var appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var assembliesToBeExcluded = boltOnIoCOptions.AssemblyOptions
			                                             .AssembliesToBeExcluded.Select(s => s.GetName().FullName).ToList();
			var assemblies = GetAssembliesThatStartsWith("BoltOn");
			var appPrefix = _callingAssembly.GetName().Name.Split('.')[0];
			var appAssemblies = GetAssembliesThatStartsWith(appPrefix);
			assemblies.AddRange(boltOnIoCOptions.AssemblyOptions.AssembliesToBeIncluded);
			assemblies.AddRange(appAssemblies);
			assemblies.Add(_callingAssembly);
			assemblies = assemblies.Distinct().ToList();
			assemblies.RemoveAll(a => assembliesToBeExcluded.Contains(a.GetName().FullName));

			var sortedAssemblies = new List<Assembly>();
			var assemblyNames = assemblies.Select(s => s.GetName());
			var boltOnAssembly = assemblies.First(a => a.GetName().Name.Equals("BoltOn"));
			sortedAssemblies.Add(boltOnAssembly);
			assemblies.Remove(boltOnAssembly);

			var iocAssemblies = assemblies.Where(a => a.GetName().Name.
												  StartsWith("BoltOn.IoC.", StringComparison.Ordinal)).ToList();
			Contract.Requires(iocAssemblies.Count > 0, "No IoC Container Adapter referenced");
			iocAssemblies.ForEach(f =>
			{
				sortedAssemblies.Add(f);
				assemblies.Remove(f);
			});

			var loggingAssemblies = assemblies.Where(a => a.GetName().Name.
													  StartsWith("BoltOn.Logging.", StringComparison.Ordinal)).ToList();
			Contract.Requires(loggingAssemblies.Count > 0, "No logging framework referenced");
			loggingAssemblies.ForEach(f =>
			{
				sortedAssemblies.Add(f);
				assemblies.Remove(f);
			});

			// load assemblies in the order of dependency
			var index = 0;
			while (assemblies.Count != 0)
			{
				var tempRefs = GetReferencedAssemblies(assemblies[index]);
				if (tempRefs.Intersect(assemblies).Count() == 0)
				{
					sortedAssemblies.Add(assemblies[index]);
					assemblies.Remove(assemblies[index]);
					index = 0;
				}
				else
					index += 1;
			}

			Assemblies = sortedAssemblies.AsReadOnly();
			if (_container == null)
				_container = CreateContainer();

			List<Assembly> GetReferencedAssemblies(Assembly assembly)
			{
				return (from r in assembly.GetReferencedAssemblies()
						join a in appDomainAssemblies
						on r.FullName equals a.FullName
						select a).Distinct().ToList();
			}

			List<Assembly> GetAssembliesThatStartsWith(string startsWith)
			{
				return (from r in referencedAssemblyNames
						join a in appDomainAssemblies
						on r.FullName equals a.FullName
						where r.Name.StartsWith(startsWith, StringComparison.Ordinal)
						select a).Distinct().ToList();
			}
		}

		private void RunRegistrationTasks()
		{
			var registrationTaskType = typeof(IBootstrapperRegistrationTask);
			var registrationTaskTypes = (from a in Assemblies
										 from t in a.GetTypes()
										 where registrationTaskType.IsAssignableFrom(t)
										 && t.IsClass
										 select t).ToList();

			var registrationTaskContext = new RegistrationTaskContext(this);
			foreach (var type in registrationTaskTypes)
			{
				var task = Activator.CreateInstance(type) as IBootstrapperRegistrationTask;
				task.Run(registrationTaskContext);
			}

			RegisterPostRegistrationTasks();
		}

		private void RegisterPostRegistrationTasks()
		{
			var registrationTaskType = typeof(IBootstrapperPostRegistrationTask);
			var registrationTaskTypes = (from a in Assemblies
										 from t in a.GetTypes()
										 where registrationTaskType.IsAssignableFrom(t)
										 && t.IsClass
										 select t).ToList();

			//foreach (var type in registrationTaskTypes)
			//{
			//	_container.RegisterTransient(registrationTaskType, type);
			//}
			_container.RegisterTransientCollection<IBootstrapperPostRegistrationTask>(registrationTaskTypes);
		}

		private void RunPostRegistrationTasks()
		{
			var postRegistrationTasks = _container.GetAllInstances<IBootstrapperPostRegistrationTask>();
			if (postRegistrationTasks != null)
				postRegistrationTasks.ToList().ForEach(t => t.Run());
		}

		private IBoltOnContainer CreateContainer()
		{
			var containerInterfaceType = typeof(IBoltOnContainer);
			var containerType = (from a in Assemblies.Where(a => a.GetName().Name.StartsWith("BoltOn.IoC.", StringComparison.Ordinal))
								 from t in a.GetTypes()
								 where containerInterfaceType.IsAssignableFrom(t)
								 && t.IsClass
								 select t).LastOrDefault();
			
			if (containerType == null)
				throw new Exception("No IoC Container Adapter referenced");
			
			var container = Activator.CreateInstance(containerType) as IBoltOnContainer;
			return container;
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
				Assemblies = null;
				_boltOnOptions.Clear();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//private void RunPreRegistrationTasks()
		//{
		//    var preRegistrationTaskType = typeof(IBootstrapperPreRegistrationTask);
		//    var preRegistrationTaskTypes = (from a in _assemblies
		//                                    from t in a.GetTypes()
		//                                    where preRegistrationTaskType.IsAssignableFrom(t)
		//                                    && t.IsClass
		//                                    select t).ToList();
		//    foreach (var type in preRegistrationTaskTypes)
		//    {
		//        var task = Activator.CreateInstance(type) as IBootstrapperPreRegistrationTask;
		//        task.Run();
		//    }
		//}
	}
}
