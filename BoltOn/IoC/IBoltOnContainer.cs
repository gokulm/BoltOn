using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;

namespace BoltOn.IoC
{
	public interface IBoltOnContainer : IServiceFactory, IDisposable
	{
		IBoltOnContainer RegisterScoped<TService, TImplementation>() where TService : class
			where TImplementation : class, TService;
		IBoltOnContainer RegisterTransient<TService, TImplementation>() where TService : class
			where TImplementation : class, TService;
		IBoltOnContainer RegisterSingleton<TService, TImplementation>() where TService : class
			where TImplementation : class, TService;
		IBoltOnContainer RegisterSingleton(Type service, object implementationInstance);
		IBoltOnContainer RegisterTransient<TService>() where TService : class;
		IBoltOnContainer RegisterTransient(Type serviceType, Type implementation);
		IBoltOnContainer RegisterTransient<TService>(Func<TService> implementationFactory) where TService : class;
		IBoltOnContainer RegisterTransientCollection(Type serviceType, IEnumerable<Type> implementationTypes);
		IBoltOnContainer RegisterTransientCollection<TService>(IEnumerable<Type> implementationTypes);
		void LockRegistration();
	}

	public class BoltOnIoCOptions
	{
		public List<string> AssembliesThatStartWith { get; set; } = new List<string>();
		public List<Assembly> AssembliesToBeExcluded { get; set; } = new List<Assembly>();
		private BoltOnIoCAssemblyOptions _assemblyOptions = new BoltOnIoCAssemblyOptions();
		private readonly Bootstrapper _bootstrapper;

		public BoltOnIoCOptions(Bootstrapper bootstrapper)
		{
			this._bootstrapper = bootstrapper;
		}

		public BoltOnIoCAssemblyOptions AssemblyOptions
		{
			get
			{
				return _assemblyOptions;
			}
			set
			{
				_assemblyOptions = value;
				PopulateAssembliesByConvention();
			}
		}

		public void PopulateAssembliesByConvention()
		{
			var assemblies = new List<Assembly>();
			var callingAssembly = _bootstrapper.CallingAssembly;
			var appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var referencedAssemblies = callingAssembly.GetReferencedAssemblies().ToList();
			referencedAssemblies.Add(callingAssembly.GetName());
			var appPrefix = callingAssembly.GetName().Name.Split('.')[0];
			AssembliesThatStartWith = (from a in AssembliesThatStartWith
									   where !a.StartsWith("BoltOn", StringComparison.Ordinal)
										&& !a.StartsWith(appPrefix, StringComparison.Ordinal)
									   select a).ToList();
			AssembliesThatStartWith.Add(appPrefix);
			// get BoltOn assemblies
			var boltOnAssemblies = GetAssemblies("BoltOn");
			boltOnAssemblies.ForEach(a => assemblies.Add(a));
			foreach (var assemblyThatStartsWith in AssembliesThatStartWith)
			{
				var appAssemblies = GetAssemblies(assemblyThatStartsWith);
				appAssemblies.ForEach(a => assemblies.Add(a));
			}
			var assembliesToBeExcludedNames = AssembliesToBeExcluded.Select(s => s.FullName).ToList();
			assemblies = assemblies.Where(a => !assembliesToBeExcludedNames.Contains(a.FullName)).Distinct().ToList();
			_bootstrapper.Assemblies = assemblies.AsReadOnly();

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
	}

	public class BoltOnIoCAssemblyOptions
	{
		public List<string> AssembliesThatStartWith { get; set; } = new List<string>();
		public List<Assembly> AssembliesToBeExcluded { get; set; } = new List<Assembly>();
	}
}
