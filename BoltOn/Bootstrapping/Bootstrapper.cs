using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.IoC;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public class Bootstrapper : IDisposable
	{
		private static readonly Lazy<Bootstrapper> _instance = new Lazy<Bootstrapper>(() => new Bootstrapper());
		private bool _isDisposed;
		private Assembly _callingAssembly;
		private Hashtable _boltOnOptions;
		private IServiceCollection _serviceCollection;
		private IServiceProvider _serviceProvider;

		private Bootstrapper()
		{
			_boltOnOptions = new Hashtable();
			Assemblies = new List<Assembly>().AsReadOnly();
			IsBolted = false;
			_serviceCollection = null;
			_serviceProvider = null;
		}

		public static Bootstrapper Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		internal IServiceCollection ServiceCollection
		{
			get
			{
				Check.Requires(_serviceCollection != null, "ServiceCollection not initialized");
				return _serviceCollection;
			}
			set
			{
				_serviceCollection = value;
			}
		}

		internal IReadOnlyList<Assembly> Assemblies { get; set; }
		public bool IsBolted { get; private set; }

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

		public void BoltOn(IServiceCollection serviceCollection, Assembly callingAssembly = null)
		{
			Check.Requires(!IsBolted, "Components are already bolted!");
			_serviceCollection = serviceCollection;
			_callingAssembly = callingAssembly ?? Assembly.GetCallingAssembly();
			LoadAssemblies();
			RunPreRegistrationTasks();
			RunRegistrationTasks();
			IsBolted = true;
		}

		internal void AddOptions<TOptionType>(TOptionType options) where TOptionType : class
		{
			Check.Requires(!IsBolted, "Components are already bolted! Options cannot be added");
			var typeName = typeof(TOptionType).Name;
			if (_boltOnOptions.ContainsKey(typeName))
				_boltOnOptions[typeName] = options;
			else
				_boltOnOptions.Add(typeName, options);
		}

		internal void RunPostRegistrationTasks(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			var registrationTaskContext = new RegistrationTaskContext(this);
			var postRegistrationTasks = serviceProvider.GetService<IEnumerable<IBootstrapperPostRegistrationTask>>();
			postRegistrationTasks.ToList().ForEach(t => t.Run(registrationTaskContext));
		}

		private void LoadAssemblies()
		{
			var boltOnIoCOptions = GetOptions<BoltOnIoCOptions>();
			var referencedAssemblyNames = _callingAssembly.GetReferencedAssemblies().ToList();
			var appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var assembliesToBeExcluded = boltOnIoCOptions
														 .AssembliesToBeExcluded.Select(s => s.GetName().FullName).ToList();
			var assemblies = GetAssembliesThatStartsWith("BoltOn");
			var appPrefix = _callingAssembly.GetName().Name.Split('.')[0];
			var appAssemblies = GetAssembliesThatStartsWith(appPrefix);
			assemblies.AddRange(boltOnIoCOptions.AssembliesToBeIncluded);
			assemblies.AddRange(appAssemblies);
			assemblies.Add(_callingAssembly);
			assemblies = assemblies.Distinct().ToList();
			assemblies.RemoveAll(a => assembliesToBeExcluded.Contains(a.GetName().FullName));

			var sortedAssemblies = new List<Assembly>();
			var assemblyNames = assemblies.Select(s => s.GetName());
			var boltOnAssembly = assemblies.First(a => a.GetName().Name.Equals("BoltOn"));
			sortedAssemblies.Add(boltOnAssembly);
			assemblies.Remove(boltOnAssembly);

			var loggingAssemblies = assemblies.Where(a => a.GetName().Name.
													  StartsWith("BoltOn.Logging.", StringComparison.Ordinal)).ToList();
			Check.Requires(loggingAssemblies.Count > 0, "No logging framework referenced");
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

		private void RunPreRegistrationTasks()
		{
			var preRegistrationTaskType = typeof(IBootstrapperPreRegistrationTask);
			var preRegistrationTaskTypes = (from a in Assemblies
											from t in a.GetTypes()
											where preRegistrationTaskType.IsAssignableFrom(t)
											&& t.IsClass
											select t).ToList();
			var context = new PreRegistrationTaskContext(this);
			foreach (var type in preRegistrationTaskTypes)
			{
				var task = Activator.CreateInstance(type) as IBootstrapperPreRegistrationTask;
				task.Run(context);
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
			registrationTaskTypes.ForEach(r => _serviceCollection.AddTransient(registrationTaskType, r));
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing & !_isDisposed)
			{
				_serviceCollection = null;
				_serviceProvider = null;
				Assemblies = null;
				_boltOnOptions.Clear();
				IsBolted = false;
				_isDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
