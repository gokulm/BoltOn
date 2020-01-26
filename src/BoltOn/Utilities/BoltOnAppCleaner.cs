using BoltOn.Bootstrapping;

namespace BoltOn.Utilities
{
    public class BoltOnAppCleaner
    {
		private readonly Bootstrapper _bootstrapper;

		//public BoltOnAppCleaner(Bootstrapper bootstrapper)
		//{
		//	this._bootstrapper = bootstrapper;
		//}

        public  void Clean()
        {
			//Bootstrapper.Instance.RunCleanupTasks();
			//this._bootstrapper.RunCleanupTasks();
        }
    }
}
