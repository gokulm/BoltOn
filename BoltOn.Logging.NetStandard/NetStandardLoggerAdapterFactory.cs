using Microsoft.Extensions.Logging;

namespace BoltOn.Logging.NetStandard
{
    public class NetStandardLoggerAdapterFactory : IBoltOnLoggerFactory
    {
        readonly ILoggerFactory _loggerFactory;

        public NetStandardLoggerAdapterFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IBoltOnLogger<TType> Create<TType>()
        {
            return new NetStandardLoggerAdapter<TType>(_loggerFactory.CreateLogger<TType>());
        }
    }
}
