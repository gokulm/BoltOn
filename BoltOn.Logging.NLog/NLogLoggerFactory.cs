using NLog;

namespace BoltOn.Logging.NLog
{
    public sealed class NLogLoggerFactory : IBoltOnLoggerFactory
    {
        public IBoltOnLogger<TType> Create<TType>()
        {
            return new NLogLoggerAdapter<TType>(LogManager.GetLogger(typeof(TType).FullName));
        }
    }
}
