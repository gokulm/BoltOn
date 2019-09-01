namespace BoltOn.Bootstrapping
{
    public class BoltOnCleanupTask : ICleanupTask
    {
        public void Run()
        {
            BoltOnServiceLocator.Current = null;
        }
    }
}
