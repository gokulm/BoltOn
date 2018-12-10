using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	internal class Bootstrapper : IDisposable
	{
		private static readonly Lazy<Bootstrapper> _instance = new Lazy<Bootstrapper>(() => new Bootstrapper());
		private Assembly _callingAssembly;
		private IServiceCollection _serviceCollection;
		private IServiceProvider _serviceProvider;
		private BoltOnOptions _options;
		private bool _isBolted;

		private Bootstrapper()
		{
			Assemblies = new List<Assembly>().AsReadOnly();
			_isBolted = false;
			_serviceCollection = null;
			_serviceProvider = null;
			_options = null;
		}

		internal static Bootstrapper Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		internal IServiceCollection Container
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

		internal IReadOnlyList<Assembly> Assemblies { get; private set; }

		internal void BoltOn(IServiceCollection serviceCollection, BoltOnOptions options, Assembly callingAssembly = null)
		{
			Check.Requires(!_isBolted, "Components are already bolted");
			_serviceCollection = serviceCollection;
			_options = options;
			_callingAssembly = callingAssembly ?? Assembly.GetCallingAssembly();
			LoadAssemblies();
			RunPreRegistrationTasks();
			RunRegistrationTasks();
			_isBolted = true;
		}

		internal void RunPostRegistrationTasks(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			var context = new PostRegistrationTaskContext(this);
			var postRegistrationTasks = serviceProvider.GetService<IEnumerable<IBootstrapperPostRegistrationTask>>();
			var tasks = serviceProvider.GetServices<IBootstrapperPostRegistrationTask>();
			postRegistrationTasks.ToList().ForEach(t => t.Run(context));
		}

		private void LoadAssemblies()
		{
			var dotNetProductAttribute = GetProductAttribute(typeof(string).Assembly);
			var assemblies = GetReferencedAssemblies(_callingAssembly).ToList();
			var assembliesToBeExcluded = _options.AssembliesToBeExcluded.Select(s => s.GetName().FullName).ToList();
			assemblies.AddRange(_options.AssembliesToBeIncluded);
			assemblies.Add(_callingAssembly);
			assemblies = assemblies.Distinct().ToList();
			assemblies.RemoveAll(a => assembliesToBeExcluded.Contains(a.GetName().FullName));

			var sortedAssemblies = new List<Assembly>();
			var assemblyNames = assemblies.Select(s => s.GetName());
			var boltOnAssembly = assemblies.First(a => a.GetName().Name.Equals("BoltOn"));
			sortedAssemblies.Add(boltOnAssembly);
			assemblies.Remove(boltOnAssembly);

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

				var referencedAssemblyNames = assembly.GetReferencedAssemblies();
				var tempReferencedAssemblies = new List<Assembly>();
				foreach (var referencedAssemblyName in referencedAssemblyNames)
				{
					var tempAssembly = Assembly.Load(referencedAssemblyName);
					var tempProductAttr = GetProductAttribute(tempAssembly);
					if (!tempProductAttr.StartsWith("Microsoft", StringComparison.Ordinal))
						tempReferencedAssemblies.Add(tempAssembly);
				}
				return tempReferencedAssemblies;
			}

			// this is to avoid adding all the .NET framework assemblies to Assemblies
			string GetProductAttribute(Assembly assembly)
			{
				var attributeData = assembly.CustomAttributes.First(attribute => attribute.AttributeType == typeof(AssemblyProductAttribute));
				var productAttr = attributeData.ConstructorArguments[0].Value as string;
				return productAttr;
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
			if (disposing)
			{
				_serviceCollection = null;
				_serviceProvider = null;
				Assemblies = null;
				_isBolted = false;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
