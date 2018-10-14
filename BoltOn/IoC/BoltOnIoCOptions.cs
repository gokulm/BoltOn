using System;
using BoltOn.Bootstrapping;

namespace BoltOn.IoC
{
	public static class BoltOnIoCExtensions
	{
		public static Bootstrapper Configure(this Bootstrapper bootstrapper,
												Action<BoltOnIoCOptions> action)
		{
			var options = new BoltOnIoCOptions();
			action(options);
			bootstrapper.AddOptions(options);
			return bootstrapper;
		}
	}

	public class BoltOnIoCOptions
	{

		public BoltOnIoCOptions()
		{
			AssemblyOptions = new BoltOnIoCAssemblyOptions();
		}

		public IBoltOnContainer Container
		{
			set
			{
				Bootstrapper.Instance.Container = value;
			}
		}

		public BoltOnIoCAssemblyOptions AssemblyOptions
		{
			get;
			set;
		}
	}
}
