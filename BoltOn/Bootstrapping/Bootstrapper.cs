using System;
using System.Collections;
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
		private Hashtable _boltOnOptions;

		internal Assembly CallingAssembly
		{
			get
			{
				return _callingAssembly;
			}
		}

		public IReadOnlyCollection<Assembly> Assemblies { get; internal set; }

		private Bootstrapper()
		{
			_assemblies = new List<Assembly>();
			_boltOnOptions = new Hashtable();
			//CallingAssembly = Assembly.GetCallingAssembly();
		}

		public static Bootstrapper Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		public void AddOptions<TOptionType>(TOptionType options) where TOptionType : class
		{
			_boltOnOptions.Add(typeof(TOptionType).Name, options);
		}

		public TOptionType GetOptions<TOptionType>() where TOptionType : class
		{
			var configValue = _boltOnOptions[typeof(TOptionType).Name];
			return (TOptionType)Convert.ChangeType(configValue, typeof(TOptionType));
		}

		public IBoltOnContainer Container
		{
			get
			{
				if (_container == null)
					throw new Exception("Container not created");
				return _container;
			}
			internal set
			{
				_container = value;
			}
		}

		public Bootstrapper SetContainer(IBoltOnContainer boltOnContainer)
		{
			_container = boltOnContainer;
			return this;
		}

		public Bootstrapper ExcludeAssemblies(params Assembly[] assemblies)
		{
			//assemblies.ToList().ForEach(a => _assembliesToBeExcluded.Add(a));
			return this;
		}

		//private void PopulateAssembliesByConvention()
		//{
		//	var appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
		//	var referencedAssemblies = _callingAssembly.GetReferencedAssemblies().ToList();
		//	referencedAssemblies.Add(_callingAssembly.GetName());
		//	// get BoltOn assemblies
		//	var boltOnAssemblies = GetAssemblies("BoltOn");
		//	boltOnAssemblies
		//		.OrderBy(o => o.Item2)
		//		.ToList()
		//		.ForEach(f => _assemblies.Add(f.Item1));
		//	// get app assemblies
		//	// todo (medium): should allow custom app prefix. to accomodate solutions with different project names
		//	var appPrefix = _callingAssembly.GetName().Name.Split('.')[0];
		//	var appAssemblies = GetAssemblies(appPrefix);
		//	appAssemblies
		//		.OrderBy(o => o.Item2)
		//		.ToList()
		//		.ForEach(f => _assemblies.Add(f.Item1));
		//	//var assembliesToBeExcludedNames = _boltOnOptions.AssembliesToBeExcluded.Select(s => s.FullName).ToList();
		//	//_assemblies = _assemblies.Where(a => !assembliesToBeExcludedNames.Contains(a.FullName)).Distinct().ToList();

		//	List<Tuple<Assembly, int>> GetAssemblies(string startsWith)
		//	{
		//		var temp = (from r in referencedAssemblies
		//					join a in appDomainAssemblies
		//					on r.FullName equals a.FullName
		//					where r.Name.StartsWith(startsWith, StringComparison.Ordinal)
		//					let orderAttribute = a.GetCustomAttribute<AssemblyRegistrationOrderAttribute>()
		//					select new Tuple<Assembly, int>
		//					(a, orderAttribute != null ? orderAttribute.Order : int.MaxValue)).Distinct().ToList();
		//		return temp;
		//	}
		//}

		public void BoltOn()
		{
			_callingAssembly = Assembly.GetCallingAssembly();
			LoadAssemblies();
			//PopulateAssembliesByConvention();
			//CreateContainer();
			//RunPreRegistrationTasks();
			RunRegistrationTasks();
			_container.LockRegistration();
			//RunPostRegistrationTasks();
		}

		private void LoadAssemblies()
		{
			var boltOnIoCOptions = GetOptions<BoltOnIoCOptions>() ?? new BoltOnIoCOptions(this);
			var referencedAssemblyNames = _callingAssembly.GetReferencedAssemblies().ToList();
			var appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var assembliesToBeExcluded = boltOnIoCOptions.AssemblyOptions.AssembliesToBeExcluded.Select(s => s.GetName());
			_assemblies = GetAssembliesThatStartsWith("BoltOn");
			var appPrefix = _callingAssembly.GetName().Name.Split('.')[0];
			var appAssemblies = GetAssembliesThatStartsWith(appPrefix);
			_assemblies.AddRange(appAssemblies);
			_assemblies.Add(_callingAssembly);
			_assemblies = _assemblies.Distinct().ToList();

			foreach (var assembly in _assemblies)
			{
				if (assembliesToBeExcluded.Contains(assembly.GetName()))
					_assemblies.Remove(assembly);
			}

			if (boltOnIoCOptions.Container == null)
				_container = CreateContainer();
			else
				_container = boltOnIoCOptions.Container;
			ServiceLocator.SetServiceFactory(_container);

			var sortedAssemblies = new List<Assembly>();
			var assemblyNames = _assemblies.Select(s => s.GetName());
			foreach (var assembly in _assemblies)
			{
				var tempRefs = GetReferencedAssemblies(assembly);
				if (tempRefs.Intersect(_assemblies).Count() == 0)
				{
					sortedAssemblies.Add(assembly);
					_assemblies.Remove(assembly);
				}
			}

			List<Assembly> GetReferencedAssemblies(Assembly assembly)
			{
				return (from r in assembly.GetReferencedAssemblies()
							join a in appDomainAssemblies
							on r.FullName equals a.FullName
							select a).Distinct().ToList();
			}


			List<Assembly> GetAssembliesThatStartsWith(string startsWith)
			{
				var temp = (from r in referencedAssemblyNames
							join a in appDomainAssemblies
							on r.FullName equals a.FullName
							where r.Name.StartsWith(startsWith, StringComparison.Ordinal)
							select a).Distinct().ToList();
				return temp;
			}
		}

		private void PopulateAssembliesByConvention()
		{
			var boltOnIoCOptions = GetOptions<BoltOnIoCOptions>() ?? new BoltOnIoCOptions(this);
			var assemblyOptions = boltOnIoCOptions.AssemblyOptions;
			var assemblies = new List<Assembly>();
			var appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var referencedAssemblies = _callingAssembly.GetReferencedAssemblies().ToList();

			// BoltOn
			//referencedAssemblies.Add(_callingAssembly.GetName());
			var boltOnAssembly = this.GetType().Assembly;
			assemblies.Add(boltOnAssembly);
			referencedAssemblies.Remove(boltOnAssembly.GetName());
			// BoltOn.IoC.<implementation>
			var iocAssemblies = GetAssemblies("BoltOn.IoC.");
			assemblies.AddRange(iocAssemblies);
			iocAssemblies.ForEach(a => referencedAssemblies.Remove(a.GetName()));
			// BoltOn.Logging.<implementation>
			var loggingAssemblies = GetAssemblies("BoltOn.Logging.");
			assemblies.AddRange(loggingAssemblies);
			loggingAssemblies.ForEach(a => referencedAssemblies.Remove(a.GetName()));
			var appPrefix = _callingAssembly.GetName().Name.Split('.')[0];


			assemblyOptions.AssembliesThatStartWith = (from a in assemblyOptions.AssembliesThatStartWith
													   where !a.StartsWith("BoltOn", StringComparison.Ordinal)
														&& !a.StartsWith(appPrefix, StringComparison.Ordinal)
													   select a).ToList();
			assemblyOptions.AssembliesThatStartWith.Add(appPrefix);
			// get BoltOn assemblies
			var boltOnAssemblies = GetAssemblies("BoltOn");
			boltOnAssemblies.ForEach(a => assemblies.Add(a));
			//foreach (var assemblyThatStartsWith in _assemblyOptions.AssembliesThatStartWith)
			//{
			//	var appAssemblies = GetAssemblies(assemblyThatStartsWith);
			//	appAssemblies.ForEach(a => assemblies.Add(a));
			//}
			//var assembliesToBeExcludedNames = _assemblyOptions.AssembliesToBeExcluded.Select(s => s.FullName).ToList();
			//assemblies = assemblies.Where(a => !assembliesToBeExcludedNames.Contains(a.FullName)).Distinct().ToList();
			////_bootstrapper.Assemblies = assemblies.AsReadOnly();

			List<Assembly> GetAssemblies(string startsWith)
			{
				var temp = (from r in referencedAssemblies
							join a in appDomainAssemblies
							on r.FullName equals a.FullName
							where r.Name.StartsWith(startsWith, StringComparison.Ordinal)
							select a).Distinct().ToList();
				return temp;
			}
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
			//RegisterPostRegistrationTasks();
		}

		//private void RegisterPostRegistrationTasks()
		//{
		//    var registrationTaskType = typeof(IBootstrapperPostRegistrationTask);
		//    var registrationTaskTypes = (from a in _assemblies
		//                                 from t in a.GetTypes()
		//                                 where registrationTaskType.IsAssignableFrom(t)
		//                                 && t.IsClass
		//                                 select t).ToList();

		//    //foreach (var type in registrationTaskTypes)
		//    //{
		//    //	_container.RegisterTransient(registrationTaskType, type);
		//    //}
		//    _container.RegisterTransientCollection<IBootstrapperPostRegistrationTask>(registrationTaskTypes);
		//}

		//private void RunPostRegistrationTasks()
		//{
		//    var postRegistrationTasks = _container.GetAllInstances<IBootstrapperPostRegistrationTask>();
		//    if (postRegistrationTasks != null)
		//        postRegistrationTasks.ToList().ForEach(t => t.Run());
		//}

		private IBoltOnContainer CreateContainer()
		{
			var containerInterfaceType = typeof(IBoltOnContainer);
			var containerType = (from a in _assemblies.Where(a => a.GetName().Name.StartsWith("BoltOn", StringComparison.Ordinal))
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
				_assemblies.Clear();
				//_assembliesToBeExcluded.Clear();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
