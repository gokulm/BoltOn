using BoltOn.Bootstrapping;

namespace BoltOn.Utilities
{
    public static class BoltOnAppCleaner
    {
        public static void Clean()
        {
            Bootstrapper.Instance.RunCleanupTasks();
        }
    }
}
