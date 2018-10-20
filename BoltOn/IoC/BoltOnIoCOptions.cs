using BoltOn.Bootstrapping;

namespace BoltOn.IoC
{
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
