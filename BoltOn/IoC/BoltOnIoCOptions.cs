using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;

namespace BoltOn.IoC
{
	public class BoltOnIoCOptions
	{
		private BoltOnIoCAssemblyOptions _assemblyOptions;
		private readonly Bootstrapper _bootstrapper;
		private IBoltOnContainer _boltOnContainer;

		public BoltOnIoCOptions(Bootstrapper bootstrapper)
		{
			_bootstrapper = bootstrapper;
			AssemblyOptions = new BoltOnIoCAssemblyOptions();
		}

		public IBoltOnContainer Container
		{
			get
			{
				return _boltOnContainer;
			}
			set
			{
				_boltOnContainer = value;
				_bootstrapper.Container = _boltOnContainer;
			}
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
			_assemblyOptions.AssembliesThatStartWith = (from a in _assemblyOptions.AssembliesThatStartWith
														where !a.StartsWith("BoltOn", StringComparison.Ordinal)
														 && !a.StartsWith(appPrefix, StringComparison.Ordinal)
														select a).ToList();
			_assemblyOptions.AssembliesThatStartWith.Add(appPrefix);
			// get BoltOn assemblies
			var boltOnAssemblies = GetAssemblies("BoltOn");
			boltOnAssemblies.ForEach(a => assemblies.Add(a));
			foreach (var assemblyThatStartsWith in _assemblyOptions.AssembliesThatStartWith)
			{
				var appAssemblies = GetAssemblies(assemblyThatStartsWith);
				appAssemblies.ForEach(a => assemblies.Add(a));
			}
			var assembliesToBeExcludedNames = _assemblyOptions.AssembliesToBeExcluded.Select(s => s.FullName).ToList();
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

		public void RegisterByConvention()
		{
			var container = _bootstrapper.Container;
			var interfaces = (from assembly in _bootstrapper.Assemblies
							  from type in assembly.GetTypes()
							  where type.IsInterface
							  select type).ToList();
			var registrations = (from @interface in interfaces
								 from assembly in _bootstrapper.Assemblies
								 from type in assembly.GetTypes()
								 where !type.IsAbstract
									   && type.IsClass && @interface.IsAssignableFrom(type)
									   && type.Name.Equals(@interface.Name.Substring(1))
								 select new { Interface = @interface, Implementation = type }).ToList();

			registrations.ForEach(f => container.RegisterTransient(f.Interface, f.Implementation));
			RegisterOtherTypes();
		}

		private void RegisterOtherTypes()
		{
			var container = _bootstrapper.Container;
			ServiceLocator.SetServiceFactory(container);
			container.RegisterSingleton(typeof(IServiceFactory), new ServiceFactory(container));
		}
	}
}
