using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	internal sealed class Bootstrapper : IDisposable
	{
		private static readonly Lazy<Bootstrapper> _instance = new Lazy<Bootstrapper>(() => new Bootstrapper());
		private Assembly _callingAssembly;
		private IServiceCollection _serviceCollection;
		private IServiceProvider _serviceProvider;
		private bool _isBolted, _isAppCleaned, _isTightened;
		private RegistrationTaskContext _registrationTaskContext;

		private Bootstrapper()
		{
			Assemblies = new List<Assembly>().AsReadOnly();
			_isBolted = false;
			_serviceCollection = null;
			_serviceProvider = null;
			Options = null;
		}

		internal static Bootstrapper Instance => _instance.Value;

		internal IServiceCollection Container
		{
			get
			{
				Check.Requires(_serviceCollection != null, "ServiceCollection not initialized");
				return _serviceCollection;
			}
			set => _serviceCollection = value;
		}

		internal IServiceProvider ServiceProvider
		{
			get
			{
				Check.Requires(_serviceProvider != null, "ServiceProvider not initialized");
				return _serviceProvider;
			}
		}

		internal BoltOnOptions Options
		{
			get;
			private set;
		}

		internal IReadOnlyList<Assembly> Assemblies { get; private set; }

		internal void BoltOn(IServiceCollection serviceCollection, BoltOnOptions options, Assembly callingAssembly = null)
		{
			if (_isBolted)
				return;

			_serviceCollection = serviceCollection;
			Options = options;
			_callingAssembly = callingAssembly ?? Assembly.GetCallingAssembly();
			LoadAssemblies();
			RunRegistrationTasks();
			_isBolted = true;
		}

		internal void RunPostRegistrationTasks(IServiceProvider serviceProvider)
		{
			if (_isTightened)
				return;

			_serviceProvider = serviceProvider;
			var context = new PostRegistrationTaskContext(this);
			var postRegistrationTasks = serviceProvider.GetService<IEnumerable<IPostRegistrationTask>>();
			postRegistrationTasks.ToList().ForEach(t => t.Run(context));
			_isTightened = true;
		}

		private void LoadAssemblies()
		{
			var assemblies = new List<Assembly> { Assembly.GetExecutingAssembly(), _callingAssembly };
			var sortedAssemblies = new HashSet<Assembly>();
			assemblies = assemblies.Distinct().ToList();
			assemblies.AddRange(Options.AssembliesToBeIncluded);

			// load assemblies in the order of dependency
			var index = 0;
			while (assemblies.Count != 0)
			{
				var tempRefs = GetReferencedAssemblies(assemblies[index]);
				if (!tempRefs.Intersect(assemblies).Any())
				{
					sortedAssemblies.Add(assemblies[index]);
					assemblies.Remove(assemblies[index]);
					index = 0;
				}
				else
					index += 1;
			}

			Assemblies = sortedAssemblies.ToList().AsReadOnly();

			IEnumerable<Assembly> GetReferencedAssemblies(Assembly assembly)
			{
				var referencedAssemblyNames = assembly.GetReferencedAssemblies();
				foreach (var referencedAssemblyName in referencedAssemblyNames)
				{
					var tempAssembly = Assembly.Load(referencedAssemblyName);
					yield return tempAssembly;
				}
			}
		}

		private void RunRegistrationTasks()
		{
			var registrationTaskType = typeof(IRegistrationTask);
			var registrationTaskTypes = (from a in Assemblies
										 from t in a.GetTypes()
										 where registrationTaskType.IsAssignableFrom(t)
										 && t.IsClass
										 select t).ToList();

			_registrationTaskContext = new RegistrationTaskContext(this);
			foreach (var type in registrationTaskTypes)
			{
				var task = Activator.CreateInstance(type) as IRegistrationTask;
				task?.Run(_registrationTaskContext);
			}

			FinalizeRegistrations();
			RegisterPostRegistrationTasks();
			RegisterCleanupTasks();
		}

		private void FinalizeRegistrations()
		{
			foreach (var interceptorImplementation in _registrationTaskContext.InterceptorTypes)
			{
				_serviceCollection.AddInterceptor(interceptorImplementation);
			}
		}

		private void RegisterPostRegistrationTasks()
		{
			var registrationTaskType = typeof(IPostRegistrationTask);
			var registrationTaskTypes = (from a in Assemblies
										 from t in a.GetTypes()
										 where registrationTaskType.IsAssignableFrom(t)
										 && t.IsClass
										 select t).ToList();
			registrationTaskTypes.ForEach(r => _serviceCollection.AddTransient(registrationTaskType, r));
		}

		private void RegisterCleanupTasks()
		{
			var cleanupTaskType = typeof(ICleanupTask);
			var cleanupTaskTypes = (from a in Assemblies
										 from t in a.GetTypes()
										 where cleanupTaskType.IsAssignableFrom(t)
										 && t.IsClass
										 select t).ToList();
			cleanupTaskTypes.ForEach(r => _serviceCollection.AddTransient(cleanupTaskType, r));
		}

		internal void RunCleanupTasks()
		{
			if (_serviceProvider != null && !_isAppCleaned && _isBolted)
			{
				var postRegistrationTasks = _serviceProvider.GetService<IEnumerable<ICleanupTask>>();
				postRegistrationTasks.Reverse().ToList().ForEach(t => t.Run());
				_isAppCleaned = true;
			}
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				RunCleanupTasks();
				_serviceCollection = null;
				BoltOnServiceLocator.Current = null;
				_serviceProvider = null;
				_registrationTaskContext = null;
				Assemblies = null;
				_callingAssembly = null;
				Options = null;
				_isBolted = false;
				_isAppCleaned = false;
				_isTightened = false;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
