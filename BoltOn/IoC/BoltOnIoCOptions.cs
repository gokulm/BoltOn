using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoltOn.Bootstrapping;

namespace BoltOn.IoC
{
	public static class BoltOnIoCExtensions
	{
		public static Bootstrapper ConfigureIoC(this Bootstrapper bootstrapper,
												Action<BoltOnIoCOptions> action = null)
		{
			var options = new BoltOnIoCOptions(bootstrapper);
			action?.Invoke(options);
			bootstrapper.AddOptions(options);
			return bootstrapper;
		}
	}

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
			}
		}
	}
}
